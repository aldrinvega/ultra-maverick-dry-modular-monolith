using MediatR;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

public sealed class GetPurchaseOrdersQueryHandler
    : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IProcurementUnitOfWork _uow;

    public GetPurchaseOrdersQueryHandler(IProcurementUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page > 10_000 ? 10_000 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var orders = await _uow.PurchaseOrders.ListAsync(request.Search, skip, pageSize, ct);
        var total = await _uow.PurchaseOrders.CountAsync(request.Search, ct);

        var dtos = orders
            .Select(o => new PurchaseOrderDto(
                o.Id,
                o.PoNumber,
                o.PrNumber,
                o.PoDate,
                o.PrDate,
                o.SupplierId,
                o.ImportBatchId,
                o.Status,
                o.IsActive,
                o.CreatedAtUtc))
            .ToList();

        return new PagedResult<PurchaseOrderDto>(dtos, page, pageSize, total);
    }
}
