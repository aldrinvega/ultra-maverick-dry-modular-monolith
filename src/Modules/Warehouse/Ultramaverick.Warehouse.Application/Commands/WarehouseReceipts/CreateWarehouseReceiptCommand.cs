using MediatR;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Commands.WarehouseReceipts;

public sealed record CreateWarehouseReceiptCommand(
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Quantity,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate) : IRequest<Result<int>>;
