using Ultramaverick.Orders.Application.Abstractions;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class OrdersUnitOfWork : IOrdersUnitOfWork
{
    private readonly OrdersDbContext _context;

    public OrdersUnitOfWork(OrdersDbContext context)
    {
        _context = context;

        Orders = new OrderRepository(context);
    }

    public IOrderRepository Orders { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
