using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.UnitTests.Fakes;

internal sealed class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<int, Order> _byId = new();
    private readonly List<Order> _added = new();
    private int _nextId;

    public IReadOnlyList<Order> All => _added;

    /// <summary>Registers an order under an explicit id, as if it had already been persisted.</summary>
    public void Seed(int id, Order order)
    {
        SetId(order, id);
        _byId[id] = order;
    }

    public Task<Order?> GetByIdAsync(int orderId, CancellationToken ct)
        => Task.FromResult(_byId.TryGetValue(orderId, out var order) ? order : null);

    public Task AddAsync(Order order, CancellationToken ct)
    {
        // Emulate the identity value EF assigns on save so handlers can return a real id.
        var id = ++_nextId;
        SetId(order, id);
        _added.Add(order);
        _byId[id] = order;
        return Task.CompletedTask;
    }

    private static void SetId(Order order, int id)
    {
        var setter = typeof(Order).GetProperty(nameof(Order.Id))!.GetSetMethod(nonPublic: true)!;
        setter.Invoke(order, new object[] { id });
    }
}

internal sealed class FakeOrdersUnitOfWork : IOrdersUnitOfWork
{
    public FakeOrderRepository OrdersRepo { get; } = new();

    public int SaveCount { get; private set; }

    public IOrderRepository Orders => OrdersRepo;

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}

internal sealed class FakeOrderNumberGenerator : IOrderNumberGenerator
{
    public string Next { get; set; } = "SO-20260101-0001";

    public Task<string> NextAsync(DateTime utcNow, CancellationToken ct)
        => Task.FromResult(Next);
}

internal sealed class FakeCustomerLookup : ICustomerLookup
{
    public HashSet<int> ExistingCustomerIds { get; } = new();

    public Task<bool> ExistsAsync(int customerId, CancellationToken ct)
        => Task.FromResult(ExistingCustomerIds.Contains(customerId));
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}
