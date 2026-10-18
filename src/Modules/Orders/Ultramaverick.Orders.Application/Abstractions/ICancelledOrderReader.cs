using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Application.Queries.CancelledOrders;

namespace Ultramaverick.Orders.Application.Abstractions;

public interface ICancelledOrderReader
{
    Task<IReadOnlyList<CancelledOrderDto>> GetByCustomerAsync(int customerId, CancellationToken ct);
    Task<PagedResult<CancelledOrderDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
}
