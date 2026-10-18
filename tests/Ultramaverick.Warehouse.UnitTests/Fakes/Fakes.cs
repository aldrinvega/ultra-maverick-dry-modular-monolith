using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain.Entities;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.UnitTests.Fakes;

internal sealed class FakeWarehouseRepository : IWarehouseRepository
{
    private readonly List<WarehouseEntity> _warehouses = new();

    public IReadOnlyList<WarehouseEntity> All => _warehouses;

    public void Seed(WarehouseEntity warehouse) => _warehouses.Add(warehouse);

    public Task<WarehouseEntity?> GetByIdAsync(int warehouseId, CancellationToken ct)
        => Task.FromResult(_warehouses.FirstOrDefault(w => w.Id == warehouseId));

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => Task.FromResult(_warehouses.Any(w => w.Code == code));

    public Task<IReadOnlyList<WarehouseEntity>> ListAsync(bool? isActive, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<WarehouseEntity>>(
            _warehouses.Where(w => isActive is null || w.IsActive == isActive).ToList());

    public Task AddAsync(WarehouseEntity warehouse, CancellationToken ct)
    {
        _warehouses.Add(warehouse);
        return Task.CompletedTask;
    }
}

internal sealed class FakeWarehouseReceiptRepository : IWarehouseReceiptRepository
{
    private readonly List<WarehouseReceipt> _receipts = new();

    public IReadOnlyList<WarehouseReceipt> All => _receipts;

    public void Seed(WarehouseReceipt receipt) => _receipts.Add(receipt);

    public Task<bool> ExistsForQcReceiptAsync(int qcReceiptId, CancellationToken ct)
        => Task.FromResult(_receipts.Any(r => r.QcReceiptId == qcReceiptId));

    public Task<WarehouseReceipt?> GetByIdAsync(int warehouseReceiptId, CancellationToken ct)
        => Task.FromResult(_receipts.FirstOrDefault(r => r.Id == warehouseReceiptId));

    public Task<IReadOnlyList<WarehouseReceipt>> ListAsync(
        int skip, int take, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<WarehouseReceipt>>(
            _receipts.Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(CancellationToken ct)
        => Task.FromResult(_receipts.Count);

    public Task AddAsync(WarehouseReceipt receipt, CancellationToken ct)
    {
        _receipts.Add(receipt);
        return Task.CompletedTask;
    }
}

internal sealed class FakeInventoryBalanceRepository : IInventoryBalanceRepository
{
    private readonly List<InventoryBalance> _balances = new();

    public IReadOnlyList<InventoryBalance> All => _balances;

    public void Seed(InventoryBalance balance) => _balances.Add(balance);

    public Task<InventoryBalance?> GetForUpdateAsync(
        int warehouseId, int itemId, int uomId, string? lotNo, CancellationToken ct)
        => Task.FromResult(_balances.FirstOrDefault(b =>
            b.WarehouseId == warehouseId && b.ItemId == itemId && b.UomId == uomId && b.LotNo == lotNo));

    public Task<IReadOnlyList<InventoryBalance>> ListAsync(int? warehouseId, int? itemId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<InventoryBalance>>(
            _balances.Where(b => (warehouseId is null || b.WarehouseId == warehouseId)
                              && (itemId is null || b.ItemId == itemId)).ToList());

    public Task AddAsync(InventoryBalance balance, CancellationToken ct)
    {
        _balances.Add(balance);
        return Task.CompletedTask;
    }
}

internal sealed class FakeInventoryMovementRepository : IInventoryMovementRepository
{
    private readonly List<InventoryMovement> _movements = new();

    public IReadOnlyList<InventoryMovement> All => _movements;

    public Task AddAsync(InventoryMovement movement, CancellationToken ct)
    {
        _movements.Add(movement);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<InventoryMovement>> ListForAsync(int warehouseId, int itemId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<InventoryMovement>>(
            _movements.Where(m => m.WarehouseId == warehouseId && m.ItemId == itemId).ToList());
}

internal sealed class FakeWarehouseUnitOfWork : IWarehouseUnitOfWork
{
    public FakeWarehouseRepository WarehousesRepo { get; } = new();
    public FakeWarehouseReceiptRepository ReceiptsRepo { get; } = new();
    public FakeInventoryBalanceRepository BalancesRepo { get; } = new();
    public FakeInventoryMovementRepository MovementsRepo { get; } = new();

    public int SaveCount { get; private set; }

    public IWarehouseRepository Warehouses => WarehousesRepo;
    public IWarehouseReceiptRepository Receipts => ReceiptsRepo;
    public IInventoryBalanceRepository Balances => BalancesRepo;
    public IInventoryMovementRepository Movements => MovementsRepo;

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}

internal sealed class FakeWarehouseSelector : IWarehouseSelector
{
    public int WarehouseId { get; set; } = 1;

    public Task<int> DefaultForItemAsync(int itemId, CancellationToken ct)
        => Task.FromResult(WarehouseId);
}

internal sealed class FakeProcessedEventStore : IProcessedEventStore
{
    private readonly HashSet<(Guid EventId, string Projection)> _processed = new();

    public int MarkCount { get; private set; }

    public Task<bool> HasProcessedAsync(Guid eventId, string projection, CancellationToken ct)
        => Task.FromResult(_processed.Contains((eventId, projection)));

    public Task MarkProcessedAsync(Guid eventId, string projection, CancellationToken ct)
    {
        _processed.Add((eventId, projection));
        MarkCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}
