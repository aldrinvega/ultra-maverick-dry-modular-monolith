using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Warehouse.Application.Queries.Expiring;

public sealed record GetExpiringReceiptsQuery(int Days, int Page, int PageSize)
    : IRequest<PagedResult<WarehouseReceiptDetailDto>>;

public sealed class GetExpiringReceiptsQueryHandler
    : IRequestHandler<GetExpiringReceiptsQuery, PagedResult<WarehouseReceiptDetailDto>>
{
    private readonly IWarehouseReceiptReader _reader;

    public GetExpiringReceiptsQueryHandler(IWarehouseReceiptReader reader) => _reader = reader;

    public Task<PagedResult<WarehouseReceiptDetailDto>> Handle(
        GetExpiringReceiptsQuery request, CancellationToken ct)
    {
        var days = request.Days < 0 ? 0 : Math.Min(request.Days, 365);
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        return _reader.GetExpiringPagedAsync(days, page, pageSize, ct);
    }
}
