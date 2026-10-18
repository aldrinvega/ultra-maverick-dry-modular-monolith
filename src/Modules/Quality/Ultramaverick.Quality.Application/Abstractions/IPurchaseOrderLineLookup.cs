namespace Ultramaverick.Quality.Application.Abstractions;

/// <summary>
/// The purchase-order line data Quality needs to raise a QC receipt. The guide calls this
/// port <c>ICatalogItemLookup</c> and shows it returning {Id, ProductTypeId, BatchNo,
/// Quantity} — a shape only a purchase-order line can satisfy (a Catalog item carries no
/// batch or quantity), so it is implemented here as a PO-line lookup.
/// </summary>
public sealed record PurchaseOrderLineInfo(
    int PurchaseOrderLineId,
    int ItemId,
    int UomId,
    decimal Quantity,
    int? ProductTypeId,
    string? BatchNo);

/// <summary>
/// Read-only query port into Procurement's purchase-order lines (and Catalog, when the
/// columns exist). Implemented in Quality.Persistence with a raw-SQL cross-schema read so
/// Quality stays free of a Procurement/Catalog project reference.
/// </summary>
public interface IPurchaseOrderLineLookup
{
    Task<PurchaseOrderLineInfo?> GetAsync(int purchaseOrderLineId, CancellationToken ct);
}
