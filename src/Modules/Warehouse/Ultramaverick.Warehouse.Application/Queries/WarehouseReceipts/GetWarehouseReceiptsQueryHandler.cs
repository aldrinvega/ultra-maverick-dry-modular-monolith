using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

public sealed class GetWarehouseReceiptsQueryHandler
    : IRequestHandler<GetWarehouseReceiptsQuery, PagedResult<WarehouseReceiptDto>>
{
    private readonly IWarehouseUnitOfWork _uow;

    public GetWarehouseReceiptsQueryHandler(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<WarehouseReceiptDto>> Handle(GetWarehouseReceiptsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page > 10_000 ? 10_000 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var receipts = await _uow.Receipts.ListAsync(skip, pageSize, ct);
        var total = await _uow.Receipts.CountAsync(ct);

        var dtos = receipts.Select(WarehouseReceiptDto.From).ToList();

        return new PagedResult<WarehouseReceiptDto>(dtos, page, pageSize, total);
    }
}
