namespace Ultramaverick.Procurement.Application.Abstractions;

/// <summary>
/// Read-only query port into the Catalog module's suppliers. Implemented in
/// Procurement.Persistence; keeps Procurement free of a Catalog project reference.
/// </summary>
public interface ISupplierLookup
{
    Task<bool> ExistsAsync(int supplierId, CancellationToken ct);
}
