using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class WarehouseUnitOfWork : IWarehouseUnitOfWork
{
    private readonly WarehouseDbContext _context;

    public WarehouseUnitOfWork(WarehouseDbContext context)
    {
        _context = context;

        Warehouses = new WarehouseRepository(context);
        Receipts = new WarehouseReceiptRepository(context);
        Balances = new InventoryBalanceRepository(context);
        Movements = new InventoryMovementRepository(context);
    }

    public IWarehouseRepository Warehouses { get; }
    public IWarehouseReceiptRepository Receipts { get; }
    public IInventoryBalanceRepository Balances { get; }
    public IInventoryMovementRepository Movements { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
