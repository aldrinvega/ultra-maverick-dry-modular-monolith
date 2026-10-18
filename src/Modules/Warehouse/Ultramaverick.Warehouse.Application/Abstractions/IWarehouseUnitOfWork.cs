using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Abstractions;

/// <summary>Aggregates every Warehouse repository behind one transaction boundary.</summary>
public interface IWarehouseUnitOfWork
{
    IWarehouseRepository Warehouses { get; }
    IWarehouseReceiptRepository Receipts { get; }
    IInventoryBalanceRepository Balances { get; }
    IInventoryMovementRepository Movements { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
