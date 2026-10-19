using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Application.Queries.Warehouses;

public sealed record WarehouseSummaryDto(
    int Id,
    string Code,
    string Name,
    string? Address,
    bool IsActive);

public sealed record GetWarehousesQuery(bool? IsActive)
    : IRequest<IReadOnlyList<WarehouseSummaryDto>>;

public sealed record GetWarehouseByIdQuery(int Id)
    : IRequest<WarehouseSummaryDto?>;

public sealed class GetWarehousesQueryHandler
    : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseSummaryDto>>
{
    private readonly IWarehouseUnitOfWork _uow;

    public GetWarehousesQueryHandler(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<WarehouseSummaryDto>> Handle(
        GetWarehousesQuery request, CancellationToken ct)
    {
        var warehouses = await _uow.Warehouses.ListAsync(request.IsActive, ct);

        return warehouses
            .Select(w => new WarehouseSummaryDto(w.Id, w.Code, w.Name, w.Address, w.IsActive))
            .ToList();
    }
}

public sealed class GetWarehouseByIdQueryHandler
    : IRequestHandler<GetWarehouseByIdQuery, WarehouseSummaryDto?>
{
    private readonly IWarehouseUnitOfWork _uow;

    public GetWarehouseByIdQueryHandler(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task<WarehouseSummaryDto?> Handle(
        GetWarehouseByIdQuery request, CancellationToken ct)
    {
        if (request.Id <= 0)
            return null;

        var warehouse = await _uow.Warehouses.GetByIdAsync(request.Id, ct);

        return warehouse is null
            ? null
            : new WarehouseSummaryDto(
                warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Address, warehouse.IsActive);
    }
}
