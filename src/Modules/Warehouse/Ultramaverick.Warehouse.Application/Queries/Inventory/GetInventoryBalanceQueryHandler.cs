using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.Inventory;

public sealed class GetInventoryBalanceQueryHandler
    : IRequestHandler<GetInventoryBalanceQuery, IReadOnlyList<InventoryBalanceDto>>
{
    private readonly IWarehouseUnitOfWork _uow;

    public GetInventoryBalanceQueryHandler(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<InventoryBalanceDto>> Handle(GetInventoryBalanceQuery request, CancellationToken ct)
    {
        var balances = await _uow.Balances.ListAsync(request.WarehouseId, request.ItemId, ct);
        return balances.Select(InventoryBalanceDto.From).ToList();
    }
}
