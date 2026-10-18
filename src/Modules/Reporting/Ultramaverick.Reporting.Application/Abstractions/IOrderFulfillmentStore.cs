using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Application.Abstractions;

public interface IOrderFulfillmentStore
{
    Task<OrderFulfillment?> FindAsync(string orderNumber, CancellationToken ct);
    Task AddAsync(OrderFulfillment fulfillment, CancellationToken ct);
}
