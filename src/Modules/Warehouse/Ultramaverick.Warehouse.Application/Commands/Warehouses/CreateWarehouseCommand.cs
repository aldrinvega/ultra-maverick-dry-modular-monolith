using MediatR;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Commands.Warehouses;

public sealed record CreateWarehouseCommand(
    string Code,
    string Name,
    int? WarehouseTypeId,
    string? Address) : IRequest<Result<int>>;
