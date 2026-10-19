using MediatR;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Models;

namespace Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

public sealed record PurchaseOrderLineDto(
    int Id,
    int ItemId,
    int UomId,
    decimal OrderedQuantity,
    decimal DeliveredQuantity,
    decimal BilledQuantity,
    decimal UnitPrice,
    bool IsActive);

public sealed record PurchaseOrderDetailDto(
    int Id,
    string PoNumber,
    string? PrNumber,
    DateTime? PoDate,
    DateTime? PrDate,
    int SupplierId,
    string Status,
    bool IsActive,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record GetPurchaseOrderByIdQuery(int PurchaseOrderId)
    : IRequest<Result<PurchaseOrderDetailDto>>;

public sealed class GetPurchaseOrderByIdQueryHandler
    : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDetailDto>>
{
    private readonly IPurchaseOrderReader _reader;

    public GetPurchaseOrderByIdQueryHandler(IPurchaseOrderReader reader) => _reader = reader;

    public async Task<Result<PurchaseOrderDetailDto>> Handle(
        GetPurchaseOrderByIdQuery request, CancellationToken ct)
    {
        if (request.PurchaseOrderId <= 0)
            return Result<PurchaseOrderDetailDto>.Failure("Purchase order id must be greater than zero.");

        var order = await _reader.GetByIdAsync(request.PurchaseOrderId, ct);

        return order is null
            ? Result<PurchaseOrderDetailDto>.Failure("Purchase order not found.")
            : Result<PurchaseOrderDetailDto>.Success(order);
    }
}
