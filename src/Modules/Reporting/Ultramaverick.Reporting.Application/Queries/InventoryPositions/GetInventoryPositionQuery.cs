using MediatR;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Application.Models;

namespace Ultramaverick.Reporting.Application.Queries.InventoryPositions;

/// <summary>Read model row returned by the inventory dashboard. Eventually consistent: it must
/// not be treated as the source of truth for a write the caller has just performed.</summary>
public sealed record InventoryPositionDto(
    int WarehouseId,
    int ItemId,
    int UomId,
    string LotNo,
    decimal Quantity,
    DateTime UpdatedAtUtc);

public sealed record GetInventoryPositionQuery(int? WarehouseId)
    : IRequest<Result<IReadOnlyList<InventoryPositionDto>>>;

public sealed class GetInventoryPositionQueryHandler
    : IRequestHandler<GetInventoryPositionQuery, Result<IReadOnlyList<InventoryPositionDto>>>
{
    private readonly IInventoryPositionReader _reader;

    public GetInventoryPositionQueryHandler(IInventoryPositionReader reader) => _reader = reader;

    public async Task<Result<IReadOnlyList<InventoryPositionDto>>> Handle(
        GetInventoryPositionQuery request, CancellationToken ct)
    {
        var positions = await _reader.GetAsync(request.WarehouseId, ct);
        return Result<IReadOnlyList<InventoryPositionDto>>.Success(positions);
    }
}
