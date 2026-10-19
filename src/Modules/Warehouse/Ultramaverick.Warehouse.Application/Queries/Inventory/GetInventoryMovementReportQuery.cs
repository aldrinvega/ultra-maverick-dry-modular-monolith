using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Application.Queries.Inventory;

public sealed record InventoryMovementRowDto(
    int ItemId,
    int WarehouseId,
    decimal TotalIn,
    decimal TotalOut,
    decimal Net);

public sealed record GetInventoryMovementReportQuery(DateTime? FromUtc, DateTime? ToUtc)
    : IRequest<IReadOnlyList<InventoryMovementRowDto>>;

public sealed class GetInventoryMovementReportQueryHandler
    : IRequestHandler<GetInventoryMovementReportQuery, IReadOnlyList<InventoryMovementRowDto>>
{
    private readonly IInventoryMovementReader _reader;

    public GetInventoryMovementReportQueryHandler(IInventoryMovementReader reader) => _reader = reader;

    public Task<IReadOnlyList<InventoryMovementRowDto>> Handle(
        GetInventoryMovementReportQuery request, CancellationToken ct)
        => _reader.GetAsync(request.FromUtc, request.ToUtc, ct);
}
