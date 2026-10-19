using System.Globalization;
using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Import;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

/// <summary>
/// Imports the legacy transformation (formula) workbook. Every row is validated against the same
/// <see cref="CreateFormulaCommandValidator"/> and <see cref="AddFormulaRequirementCommandValidator"/>
/// the manual commands use; the whole workbook is committed in one save only when no row fails.
/// Groups whose (item, version) formula already exists are reported as skipped, so a re-upload is
/// idempotent.
///
/// Legacy layout (row one is the header row):
/// Formula Code, Formula Name, Version, Item Code, Item Description, Quantity.
///
/// Mapping notes:
/// - A formula header pairs a produced item with a version; the workbook's Formula Code is
///   resolved to that item and Formula Name is verified against its description.
/// - Each row's Item Code and Item Description identify the raw-material requirement. The header
///   quantity is the sum of its requirements, matching the invariant the manual formula command
///   enforces.
/// </summary>
public sealed class ImportFormulasCommandHandler
    : IRequestHandler<ImportFormulasCommand, Result<ImportReport>>
{
    private const int FormulaCodeColumn = 0;
    private const int FormulaNameColumn = 1;
    private const int VersionColumn = 2;
    private const int ItemCodeColumn = 3;
    private const int ItemDescriptionColumn = 4;
    private const int QuantityColumn = 5;

    private readonly IImportReader _reader;
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly CreateFormulaCommandValidator _formulaValidator = new();
    private readonly AddFormulaRequirementCommandValidator _requirementValidator = new();

    public ImportFormulasCommandHandler(
        IImportReader reader,
        ICatalogUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _reader = reader;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<ImportReport>> Handle(ImportFormulasCommand request, CancellationToken ct)
    {
        var rows = await _reader.ReadAsync(request.Content, ct);

        var failures = new List<ImportFailure>();
        var drafts = new List<RowDraft>(rows.Count);

        foreach (var row in rows)
        {
            var (draft, reason) = await MapAsync(row, ct);

            if (reason is not null)
                failures.Add(new ImportFailure(row.RowNumber, reason));
            else if (draft is not null)
                drafts.Add(draft);
        }

        var deduped = RejectDuplicateRequirements(drafts, failures);

        var skipped = 0;
        var toImport = new List<FormulaGroup>();

        foreach (var group in deduped.GroupBy(d => (d.HeaderItemId, d.Version)))
        {
            if (await _uow.Formulas.GetByItemAndVersionAsync(group.Key.HeaderItemId, group.Key.Version, ct)
                is not null)
            {
                skipped += group.Count();
                continue;
            }

            toImport.Add(new FormulaGroup(group.Key.HeaderItemId, group.Key.Version, group.ToList()));
        }

        // Validate-all-or-nothing: a rejected row means the workbook is not written at all.
        if (failures.Count > 0)
            return Result<ImportReport>.Success(new ImportReport(0, skipped, failures));

        foreach (var group in toImport)
        {
            var headerQuantity = group.Rows.Sum(r => r.Quantity);
            var formula = FormulaHeader.Create(
                group.HeaderItemId, group.Version, headerQuantity, _currentUser.UserId);

            foreach (var row in group.Rows)
                formula.AddRequirement(row.RawMaterialId, row.Quantity);

            await _uow.Formulas.AddAsync(formula, ct);
        }

        if (toImport.Count > 0)
            await _uow.SaveChangesAsync(ct);

        var imported = toImport.Sum(g => g.Rows.Count);
        return Result<ImportReport>.Success(
            new ImportReport(imported, skipped, Array.Empty<ImportFailure>()));
    }

    private static List<RowDraft> RejectDuplicateRequirements(
        List<RowDraft> drafts, List<ImportFailure> failures)
    {
        var deduped = new List<RowDraft>(drafts.Count);
        var seen = new HashSet<(int HeaderItemId, int Version, int RawMaterialId)>();

        foreach (var draft in drafts)
        {
            if (seen.Add((draft.HeaderItemId, draft.Version, draft.RawMaterialId)))
            {
                deduped.Add(draft);
                continue;
            }

            failures.Add(new ImportFailure(
                draft.RowNumber,
                $"Item code '{draft.RawMaterialCode}' is repeated for formula version {draft.Version}."));
        }

        return deduped;
    }

    private async Task<(RowDraft? Draft, string? Reason)> MapAsync(ImportRow row, CancellationToken ct)
    {
        var formulaCode = Value(row, FormulaCodeColumn);
        if (formulaCode is null)
            return (null, "Formula Code is required.");

        var versionText = Value(row, VersionColumn);
        if (versionText is null)
            return (null, "Version is required.");

        if (!int.TryParse(versionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
            || version <= 0)
        {
            return (null, $"Version '{versionText}' is not a positive whole number.");
        }

        var headerItem = await _uow.Items.GetByCodeAsync(formulaCode, ct);
        if (headerItem is null)
            return (null, $"Formula Code '{formulaCode}' does not match an item code.");

        var formulaName = Value(row, FormulaNameColumn);
        if (formulaName is not null
            && !string.Equals(formulaName, headerItem.ItemDescription, StringComparison.OrdinalIgnoreCase))
        {
            return (null, $"Formula Name '{formulaName}' does not match item code '{formulaCode}'.");
        }

        var rawMaterialCode = Value(row, ItemCodeColumn);
        if (rawMaterialCode is null)
            return (null, "Item Code is required.");

        var rawMaterial = await _uow.Items.GetByCodeAsync(rawMaterialCode, ct);
        if (rawMaterial is null)
            return (null, $"Item code '{rawMaterialCode}' does not exist.");

        var itemDescription = Value(row, ItemDescriptionColumn);
        if (itemDescription is not null
            && !string.Equals(itemDescription, rawMaterial.ItemDescription, StringComparison.OrdinalIgnoreCase))
        {
            return (null, $"Item description '{itemDescription}' does not match item code '{rawMaterialCode}'.");
        }

        var quantityText = Value(row, QuantityColumn);
        if (quantityText is null)
            return (null, "Quantity is required.");

        if (!TryParseDecimal(quantityText, out var quantity))
            return (null, $"Quantity '{quantityText}' is not a number.");

        var draft = new RowDraft(
            row.RowNumber, headerItem.Id, version, rawMaterial.Id, rawMaterialCode, quantity);

        var headerValidation = _formulaValidator.Validate(
            new CreateFormulaCommand(draft.HeaderItemId, draft.Version, draft.Quantity));
        if (!headerValidation.IsValid)
            return (null, string.Join("; ", headerValidation.Errors.Select(e => e.ErrorMessage)));

        // The formula header has no id until it is saved, so the requirement is validated against a
        // placeholder header id; the raw-material id and quantity are what the manual rule checks.
        var requirementValidation = _requirementValidator.Validate(
            new AddFormulaRequirementCommand(1, draft.RawMaterialId, draft.Quantity));
        if (!requirementValidation.IsValid)
            return (null, string.Join("; ", requirementValidation.Errors.Select(e => e.ErrorMessage)));

        return (draft, null);
    }

    private static string? Value(ImportRow row, int column)
    {
        var text = row.Cell(column)?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static bool TryParseDecimal(string text, out decimal value)
        => decimal.TryParse(
            text,
            NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
            CultureInfo.InvariantCulture,
            out value);

    private sealed record RowDraft(
        int RowNumber,
        int HeaderItemId,
        int Version,
        int RawMaterialId,
        string RawMaterialCode,
        decimal Quantity);

    private sealed record FormulaGroup(int HeaderItemId, int Version, List<RowDraft> Rows);
}
