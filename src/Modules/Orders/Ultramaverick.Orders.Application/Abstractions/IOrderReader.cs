using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Application.Queries.Orders;

namespace Ultramaverick.Orders.Application.Abstractions;

public interface IOrderReader
{
    Task<PagedResult<OrderSummaryDto>> GetPagedAsync(
        int? customerId, string? status, int page, int pageSize, CancellationToken ct);

    Task<OrderDetailDto?> GetByIdAsync(int orderId, CancellationToken ct);

    Task<PagedResult<MoveOrderDto>> GetMoveOrdersPagedAsync(int page, int pageSize, string? status, CancellationToken ct);

    Task<PagedResult<TransactedMoveOrderDto>> GetTransactedPagedAsync(int page, int pageSize, CancellationToken ct);
}
