using Microsoft.EntityFrameworkCore;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context) => _context = context;

    public Task<Order?> GetByIdAsync(int orderId, CancellationToken ct)
        => _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
        => await _context.Orders.AddAsync(order, ct);
}
