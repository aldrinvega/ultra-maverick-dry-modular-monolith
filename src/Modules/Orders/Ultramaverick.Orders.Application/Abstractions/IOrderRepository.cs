using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
}
