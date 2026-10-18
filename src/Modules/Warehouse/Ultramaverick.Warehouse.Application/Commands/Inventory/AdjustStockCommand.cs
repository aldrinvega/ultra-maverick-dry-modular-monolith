using MediatR;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Commands.Inventory;

public sealed record AdjustStockCommand(
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Delta) : IRequest<Result>;
