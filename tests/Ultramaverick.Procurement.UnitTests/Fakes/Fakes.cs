using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.UnitTests.Fakes;

internal sealed class FakePurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly List<PurchaseOrder> _orders = new();

    public IReadOnlyList<PurchaseOrder> All => _orders;

    public void Seed(PurchaseOrder purchaseOrder) => _orders.Add(purchaseOrder);

    public Task<PurchaseOrder?> GetByNumberAsync(string poNumber, CancellationToken ct)
        => Task.FromResult(_orders.FirstOrDefault(o => o.PoNumber == poNumber));

    public Task<IReadOnlyList<PurchaseOrder>> ListAsync(string? search, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken ct)
    {
        _orders.Add(purchaseOrder);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSupplierLookup : ISupplierLookup
{
    public HashSet<int> ExistingSupplierIds { get; } = new();

    public Task<bool> ExistsAsync(int supplierId, CancellationToken ct)
        => Task.FromResult(ExistingSupplierIds.Contains(supplierId));
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}

internal sealed class FakeProcurementUnitOfWork : IProcurementUnitOfWork
{
    public FakePurchaseOrderRepository PurchaseOrdersRepo { get; } = new();

    public int SaveCount { get; private set; }

    public IPurchaseOrderRepository PurchaseOrders => PurchaseOrdersRepo;

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}
