using MediatR;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;

namespace Ultramaverick.Orders.Application.Queries.CancelledOrders;

public sealed record GetCancelledOrdersPagedQuery(int Page, int PageSize)
    : IRequest<Result<PagedResult<CancelledOrderDto>>>;

public sealed class GetCancelledOrdersPagedQueryHandler
    : IRequestHandler<GetCancelledOrdersPagedQuery, Result<PagedResult<CancelledOrderDto>>>
{
    private const int MaxPageSize = 100;

    private readonly ICancelledOrderReader _reader;

    public GetCancelledOrdersPagedQueryHandler(ICancelledOrderReader reader) => _reader = reader;

    public async Task<Result<PagedResult<CancelledOrderDto>>> Handle(
        GetCancelledOrdersPagedQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, MaxPageSize);

        var paged = await _reader.GetPagedAsync(page, pageSize, ct);
        return Result<PagedResult<CancelledOrderDto>>.Success(paged);
    }
}
