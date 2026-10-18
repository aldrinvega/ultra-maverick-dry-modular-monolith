using MediatR;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.Inventory;

public sealed record GetInventoryBalanceQuery(int? WarehouseId, int? ItemId)
    : IRequest<IReadOnlyList<InventoryBalanceDto>>;
