using System.Globalization;
using MediatR;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Import;
using Ultramaverick.Procurement.Application.Models;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

/// <summary>
/// Imports the legacy PO-summary workbook. Every row is validated against the same
/// <see cref="CreatePurchaseOrderCommandValidator"/> the manual command uses; the whole
/// workbook is committed in one save only when no row fails. Rows whose PO number already
/// exists are reported as skipped instead of failing, so a re-upload is idempotent.
///
/// Legacy layout (row one is the header row):
/// PR Number, PR Date, PO Number, PO Date, Item Code, Item Description, Ordered,
/// Delivered, Billed, UOM, Unit Price, Vendor Name.
///
/// Mapping notes:
/// - Supplier, item and UOM are stored as surrogate ids, so the workbook's Vendor Name,
///   Item Code and UOM code are resolved through Catalog.
/// - Item Description is verified against the resolved Catalog item; a mismatch is rejected
///   rather than silently ignored.
/// - Delivered and Billed have no creation path on <see cref="PurchaseOrderLine"/> (the line
///   starts at zero and deliveries are recorded later), so a non-zero value is rejected
///   instead of being dropped.
/// </summary>
public sealed class ImportPurchaseOrdersCommandHandler
    : IRequestHandler<ImportPurchaseOrdersCommand, Result<ImportReport>>
{
    private const int PrNumberColumn = 0;
    private const int PrDateColumn = 1;
    private const int PoNumberColumn = 2;
    private const int PoDateColumn = 3;
    private const int ItemCodeColumn = 4;
    private const int ItemDescriptionColumn = 5;
    private const int OrderedColumn = 6;
    private const int DeliveredColumn = 7;
    private const int BilledColumn = 8;
    private const int UomColumn = 9;
    private const int UnitPriceColumn = 10;
    private const int VendorColumn = 11;

    private readonly IImportReader _reader;
    private readonly IProcurementUnitOfWork _uow;
    private readonly IPurchaseOrderImportLookup _lookup;
    private readonly ICurrentUser _currentUser;
    private readonly CreatePurchaseOrderCommandValidator _validator = new();

    public ImportPurchaseOrdersCommandHandler(
        IImportReader reader,
        IProcurementUnitOfWork uow,
        IPurchaseOrderImportLookup lookup,
        ICurrentUser currentUser)
    {
        _reader = reader;
        _uow = uow;
        _lookup = lookup;
        _currentUser = currentUser;
    }

    public async Task<Result<ImportReport>> Handle(ImportPurchaseOrdersCommand request, CancellationToken ct)
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

        var deduped = RejectDuplicateLines(drafts, failures);

        var skipped = 0;
        var toImport = new List<List<RowDraft>>();

        foreach (var group in deduped.GroupBy(d => d.PoNumber, StringComparer.OrdinalIgnoreCase))
        {
            if (await _uow.PurchaseOrders.GetByNumberAsync(group.Key, ct) is not null)
                skipped += group.Count();
            else
                toImport.Add(group.ToList());
        }

        // Validate-all-or-nothing: a rejected row means the workbook is not written at all.
        if (failures.Count > 0)
            return Result<ImportReport>.Success(new ImportReport(0, skipped, failures));

        foreach (var group in toImport)
        {
            var first = group[0];
            var po = PurchaseOrder.Create(
                first.PoNumber,
                first.PrNumber,
                first.PoDate,
                first.PrDate,
                first.SupplierId,
                importBatchId: null,
                _currentUser.UserId);

            foreach (var line in group)
                po.AddLine(line.ItemId, line.UomId, line.Quantity, line.UnitPrice);

            await _uow.PurchaseOrders.AddAsync(po, ct);
        }

        if (toImport.Count > 0)
            await _uow.SaveChangesAsync(ct);

        var imported = toImport.Sum(g => g.Count);
        return Result<ImportReport>.Success(
            new ImportReport(imported, skipped, Array.Empty<ImportFailure>()));
    }

    private static List<RowDraft> RejectDuplicateLines(
        List<RowDraft> drafts, List<ImportFailure> failures)
    {
        var deduped = new List<RowDraft>(drafts.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var draft in drafts)
        {
            if (seen.Add($"{draft.PoNumber}\u0000{draft.ItemCode}"))
            {
                deduped.Add(draft);
                continue;
            }

            failures.Add(new ImportFailure(
                draft.RowNumber,
                $"Item '{draft.ItemCode}' is repeated for PO '{draft.PoNumber}' in this workbook."));
        }

        return deduped;
    }

    private async Task<(RowDraft? Draft, string? Reason)> MapAsync(ImportRow row, CancellationToken ct)
    {
        var poNumber = Value(row, PoNumberColumn);
        if (poNumber is null)
            return (null, "PO Number is required.");

        var prNumber = Value(row, PrNumberColumn);

        if (!TryParseDate(Value(row, PoDateColumn), out var poDate))
            return (null, $"PO Date '{Value(row, PoDateColumn)}' is not a valid date.");

        if (!TryParseDate(Value(row, PrDateColumn), out var prDate))
            return (null, $"PR Date '{Value(row, PrDateColumn)}' is not a valid date.");

        var itemCode = Value(row, ItemCodeColumn);
        if (itemCode is null)
            return (null, "Item Code is required.");

        var uomCode = Value(row, UomColumn);
        if (uomCode is null)
            return (null, "UOM is required.");

        var vendorName = Value(row, VendorColumn);
        if (vendorName is null)
            return (null, "Vendor Name is required.");

        var orderedText = Value(row, OrderedColumn);
        if (orderedText is null)
            return (null, "Ordered quantity is required.");

        if (!TryParseDecimal(orderedText, out var ordered))
            return (null, $"Ordered quantity '{orderedText}' is not a number.");

        var unitPriceText = Value(row, UnitPriceColumn);
        if (unitPriceText is null)
            return (null, "Unit Price is required.");

        if (!TryParseDecimal(unitPriceText, out var unitPrice))
            return (null, $"Unit Price '{unitPriceText}' is not a number.");

        var deliveredText = Value(row, DeliveredColumn);
        if (deliveredText is not null)
        {
            if (!TryParseDecimal(deliveredText, out var delivered))
                return (null, $"Delivered quantity '{deliveredText}' is not a number.");

            if (delivered != 0m)
                return (null, "Delivered quantity has no target home on a new purchase-order line.");
        }

        var billedText = Value(row, BilledColumn);
        if (billedText is not null)
        {
            if (!TryParseDecimal(billedText, out var billed))
                return (null, $"Billed quantity '{billedText}' is not a number.");

            if (billed != 0m)
                return (null, "Billed quantity has no target home on a new purchase-order line.");
        }

        var supplierId = await _lookup.FindSupplierIdByNameAsync(vendorName, ct);
        if (supplierId is null)
            return (null, $"Supplier '{vendorName}' does not exist.");

        var item = await _lookup.FindItemByCodeAsync(itemCode, ct);
        if (item is null)
            return (null, $"Item code '{itemCode}' does not exist.");

        var itemDescription = Value(row, ItemDescriptionColumn);
        if (itemDescription is not null
            && !string.Equals(itemDescription, item.Description, StringComparison.OrdinalIgnoreCase))
        {
            return (null, $"Item description '{itemDescription}' does not match item code '{itemCode}'.");
        }

        var uomId = await _lookup.FindUomIdByCodeAsync(uomCode, ct);
        if (uomId is null)
            return (null, $"UOM code '{uomCode}' does not exist.");

        var draft = new RowDraft(
            row.RowNumber, poNumber, prNumber, poDate, prDate, supplierId.Value,
            itemCode, item.Id, uomId.Value, ordered, unitPrice);

        var command = new CreatePurchaseOrderCommand(
            draft.PoNumber,
            draft.PrNumber,
            draft.PoDate,
            draft.PrDate,
            draft.SupplierId,
            new List<PurchaseOrderLineInput>
            {
                new(draft.ItemId, draft.UomId, draft.Quantity, draft.UnitPrice)
            });

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

    private static bool TryParseDate(string? text, out DateTime? value)
    {
        value = null;

        if (text is null)
            return true;

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static bool TryParseDecimal(string text, out decimal value)
        => decimal.TryParse(
            text,
            NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
            CultureInfo.InvariantCulture,
            out value);

    private sealed record RowDraft(
        int RowNumber,
        string PoNumber,
        string? PrNumber,
        DateTime? PoDate,
        DateTime? PrDate,
        int SupplierId,
        string ItemCode,
        int ItemId,
        int UomId,
        decimal Quantity,
        decimal UnitPrice);
}
