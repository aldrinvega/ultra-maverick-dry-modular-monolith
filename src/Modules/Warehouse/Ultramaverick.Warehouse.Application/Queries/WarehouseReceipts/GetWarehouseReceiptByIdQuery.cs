using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

public sealed record WarehouseReceiptDetailDto(
    int Id,
    int? QcReceiptId,
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Quantity,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    DateTime ReceivedAtUtc,
    int? ReceivedByUserId,
    bool IsActive);

public sealed record GetWarehouseReceiptByIdQuery(int ReceiptId)
    : IRequest<Result<WarehouseReceiptDetailDto>>;

public sealed class GetWarehouseReceiptByIdQueryHandler
    : IRequestHandler<GetWarehouseReceiptByIdQuery, Result<WarehouseReceiptDetailDto>>
{
    private readonly IWarehouseReceiptReader _reader;

    public GetWarehouseReceiptByIdQueryHandler(IWarehouseReceiptReader reader) => _reader = reader;

    public async Task<Result<WarehouseReceiptDetailDto>> Handle(
        GetWarehouseReceiptByIdQuery request, CancellationToken ct)
    {
        if (request.ReceiptId <= 0)
            return Result<WarehouseReceiptDetailDto>.Failure("Receipt id must be greater than zero.");

        var receipt = await _reader.GetByIdAsync(request.ReceiptId, ct);

        return receipt is null
            ? Result<WarehouseReceiptDetailDto>.Failure("Warehouse receipt not found.")
            : Result<WarehouseReceiptDetailDto>.Success(receipt);
    }
}
