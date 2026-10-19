namespace Ultramaverick.Procurement.Application.Abstractions;

/// <summary>
/// Read-only resolution of the Catalog codes a legacy PO-summary workbook carries
/// (item code, uom code, supplier name) to the surrogate ids Procurement stores.
/// Implemented in Procurement.Persistence so Procurement keeps no Catalog reference.
/// </summary>
public interface IPurchaseOrderImportLookup
{
    Task<int?> FindSupplierIdByNameAsync(string supplierName, CancellationToken ct);
    Task<ImportedItem?> FindItemByCodeAsync(string itemCode, CancellationToken ct);
    Task<int?> FindUomIdByCodeAsync(string uomCode, CancellationToken ct);
}

/// <summary>The Catalog item an ItemCode resolves to, with its canonical description.</summary>
public sealed record ImportedItem(int Id, string Description);
