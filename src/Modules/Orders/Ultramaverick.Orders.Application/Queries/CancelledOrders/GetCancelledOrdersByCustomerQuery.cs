using MediatR;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;

namespace Ultramaverick.Orders.Application.Queries.CancelledOrders;

public sealed record CancelledOrderDto(
    int Id,
    int OrderId,
    int? CustomerId,
    int? ReasonId,
    int? CancelledByUserId,
    DateTime CancelledAtUtc);

public sealed record GetCancelledOrdersByCustomerQuery(int CustomerId)
    : IRequest<Result<IReadOnlyList<CancelledOrderDto>>>;

public sealed class GetCancelledOrdersByCustomerQueryHandler
    : IRequestHandler<GetCancelledOrdersByCustomerQuery, Result<IReadOnlyList<CancelledOrderDto>>>
{
    private readonly ICancelledOrderReader _reader;

    public GetCancelledOrdersByCustomerQueryHandler(ICancelledOrderReader reader) => _reader = reader;

    public async Task<Result<IReadOnlyList<CancelledOrderDto>>> Handle(
        GetCancelledOrdersByCustomerQuery request, CancellationToken ct)
    {
        if (request.CustomerId <= 0)
            return Result<IReadOnlyList<CancelledOrderDto>>.Failure("Customer id must be greater than zero.");

        var cancelledOrders = await _reader.GetByCustomerAsync(request.CustomerId, ct);
        return Result<IReadOnlyList<CancelledOrderDto>>.Success(cancelledOrders);
    }
}
