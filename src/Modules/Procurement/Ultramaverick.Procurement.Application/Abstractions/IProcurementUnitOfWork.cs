namespace Ultramaverick.Procurement.Application.Abstractions;

/// <summary>Aggregates every Procurement repository behind one transaction boundary.</summary>
public interface IProcurementUnitOfWork
{
    IPurchaseOrderRepository PurchaseOrders { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
