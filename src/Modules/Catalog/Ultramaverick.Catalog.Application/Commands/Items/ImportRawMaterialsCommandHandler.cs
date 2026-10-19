using System.Globalization;
using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Import;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Items;

/// <summary>
/// Imports the legacy raw-material setup workbook. Every row is validated against the same
/// <see cref="CreateItemCommandValidator"/> the manual command uses; the whole workbook is
/// committed in one save only when no row fails. Rows whose Item Code already exists are
/// reported as skipped instead of failing, so a re-upload is idempotent.
///
/// Legacy layout (row one is the header row):
/// Item Code, Item Description, Item Category, UOM, Buffer Level.
///
/// Mapping notes:
/// - Item category and UOM are stored as surrogate ids, so the workbook's category name and
///   UOM code or description are resolved through Catalog.
/// - The legacy template never carried an expiry flag, so imported items are not expirable.
/// </summary>
public sealed class ImportRawMaterialsCommandHandler
    : IRequestHandler<ImportRawMaterialsCommand, Result<ImportReport>>
{
    private const int ItemCodeColumn = 0;
    private const int ItemDescriptionColumn = 1;
    private const int ItemCategoryColumn = 2;
    private const int UomColumn = 3;
    private const int BufferLevelColumn = 4;

    private readonly IImportReader _reader;
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly CreateItemCommandValidator _validator = new();

    public ImportRawMaterialsCommandHandler(
        IImportReader reader,
        ICatalogUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _reader = reader;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<ImportReport>> Handle(ImportRawMaterialsCommand request, CancellationToken ct)
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

        var deduped = RejectDuplicateCodes(drafts, failures);

        var skipped = 0;
        var toImport = new List<RowDraft>();

        foreach (var draft in deduped)
        {
            if (await _uow.Items.GetByCodeAsync(draft.ItemCode, ct) is not null)
                skipped++;
            else
                toImport.Add(draft);
        }

        // Validate-all-or-nothing: a rejected row means the workbook is not written at all.
        if (failures.Count > 0)
            return Result<ImportReport>.Success(new ImportReport(0, skipped, failures));

        foreach (var draft in toImport)
        {
            var item = Item.Create(
                draft.ItemCode,
                draft.ItemDescription,
                draft.ItemCategoryId,
                draft.UomId,
                draft.BufferLevel,
                isExpirable: false,
                _currentUser.UserId);

            await _uow.Items.AddAsync(item, ct);
        }

        if (toImport.Count > 0)
            await _uow.SaveChangesAsync(ct);

        return Result<ImportReport>.Success(
            new ImportReport(toImport.Count, skipped, Array.Empty<ImportFailure>()));
    }

    private static List<RowDraft> RejectDuplicateCodes(
        List<RowDraft> drafts, List<ImportFailure> failures)
    {
        var deduped = new List<RowDraft>(drafts.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var draft in drafts)
        {
            if (seen.Add(draft.ItemCode))
            {
                deduped.Add(draft);
                continue;
            }

            failures.Add(new ImportFailure(
                draft.RowNumber,
                $"Item code '{draft.ItemCode}' is repeated in this workbook."));
        }

        return deduped;
    }

    private async Task<(RowDraft? Draft, string? Reason)> MapAsync(ImportRow row, CancellationToken ct)
    {
        var itemCode = Value(row, ItemCodeColumn);
        if (itemCode is null)
            return (null, "Item Code is required.");

        var itemDescription = Value(row, ItemDescriptionColumn);
        if (itemDescription is null)
            return (null, "Item Description is required.");

        var categoryName = Value(row, ItemCategoryColumn);
        if (categoryName is null)
            return (null, "Item Category is required.");

        var category = await _uow.ItemCategories.GetByNameAsync(categoryName, ct);
        if (category is null)
            return (null, $"Item category '{categoryName}' does not exist.");

        var uomText = Value(row, UomColumn);
        if (uomText is null)
            return (null, "UOM is required.");

        var uom = await _uow.Uoms.GetByCodeAsync(uomText, ct)
                  ?? await _uow.Uoms.GetByDescriptionAsync(uomText, ct);
        if (uom is null)
            return (null, $"UOM '{uomText}' does not exist.");

        var bufferText = Value(row, BufferLevelColumn);
        if (bufferText is null)
            return (null, "Buffer Level is required.");

        if (!TryParseDecimal(bufferText, out var bufferLevel))
            return (null, $"Buffer Level '{bufferText}' is not a number.");

        var draft = new RowDraft(
            row.RowNumber, itemCode, itemDescription, category.Id, uom.Id, bufferLevel);

        var command = new CreateItemCommand(
            draft.ItemCode, draft.ItemDescription, draft.ItemCategoryId, draft.UomId,
            draft.BufferLevel, IsExpirable: false);

        var validation = _validator.Validate(command);
        if (validation.IsValid)
            return (draft, null);

        return (null, string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
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
        string ItemCode,
        string ItemDescription,
        int ItemCategoryId,
        int UomId,
        decimal BufferLevel);
}
