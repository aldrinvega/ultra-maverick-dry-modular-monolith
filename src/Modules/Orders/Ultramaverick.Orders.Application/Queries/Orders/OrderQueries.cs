using MediatR;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Application.Models;

namespace Ultramaverick.Orders.Application.Queries.Orders;

public sealed record OrderSummaryDto(
    int Id,
    string OrderNumber,
    int CustomerId,
    string Status,
    DateTime? OrderDate,
    DateTime? DeliveryDate,
    bool IsActive);

public sealed record OrderLineDto(
    int Id,
    int ItemId,
    int UomId,
    decimal QuantityOrdered,
    decimal AllocatedQuantity,
    string? PlateNumber,
    string? DeliveryStatus,
    bool IsActive);

public sealed record OrderDetailDto(
    int Id,
    string OrderNumber,
    int CustomerId,
    string? CustomerPosition,
    int? FarmId,
    DateTime? OrderDate,
    DateTime? DeliveryDate,
    string Status,
    string? Remarks,
    bool IsActive,
    IReadOnlyList<OrderLineDto> Lines);

public sealed record MoveOrderDto(
    int Id,
    int? OrderNo,
    int ItemId,
    decimal QuantityOrdered,
    string Status,
    DateTime? OrderDate,
    DateTime? DateNeeded,
    bool IsActive);

public sealed record TransactedMoveOrderDto(
    int Id,
    int MoveOrderId,
    int? OrderNo,
    DateTime? DeliveryDate,
    bool IsTransact);

public sealed record GetOrdersQuery(int? CustomerId, string? Status, int Page, int PageSize)
    : IRequest<Result<PagedResult<OrderSummaryDto>>>;

public sealed record GetOrderByIdQuery(int OrderId)
    : IRequest<Result<OrderDetailDto>>;

public sealed record GetMoveOrdersQuery(int Page, int PageSize, string? Status)
    : IRequest<Result<PagedResult<MoveOrderDto>>>;

public sealed record GetTransactedMoveOrdersQuery(int Page, int PageSize)
    : IRequest<Result<PagedResult<TransactedMoveOrderDto>>>;

public sealed class GetOrdersQueryHandler
    : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private const int MaxPageSize = 100;

    private readonly IOrderReader _reader;

    public GetOrdersQueryHandler(IOrderReader reader) => _reader = reader;

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var paged = await _reader.GetPagedAsync(
            request.CustomerId, request.Status, Clamp(request.Page), ClampSize(request.PageSize), ct);

        return Result<PagedResult<OrderSummaryDto>>.Success(paged);
    }

    internal static int Clamp(int page) => page < 1 ? 1 : page;

    internal static int ClampSize(int pageSize) => pageSize < 1 ? 25 : Math.Min(pageSize, MaxPageSize);
}

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IOrderReader _reader;

    public GetOrderByIdQueryHandler(IOrderReader reader) => _reader = reader;

    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        if (request.OrderId <= 0)
            return Result<OrderDetailDto>.Failure("Order id must be greater than zero.");

        var order = await _reader.GetByIdAsync(request.OrderId, ct);
        return order is null
            ? Result<OrderDetailDto>.Failure("Order not found.")
            : Result<OrderDetailDto>.Success(order);
    }
}

public sealed class GetMoveOrdersQueryHandler
    : IRequestHandler<GetMoveOrdersQuery, Result<PagedResult<MoveOrderDto>>>
{
    private readonly IOrderReader _reader;

    public GetMoveOrdersQueryHandler(IOrderReader reader) => _reader = reader;

    public async Task<Result<PagedResult<MoveOrderDto>>> Handle(GetMoveOrdersQuery request, CancellationToken ct)
    {
        var paged = await _reader.GetMoveOrdersPagedAsync(
            GetOrdersQueryHandler.Clamp(request.Page),
            GetOrdersQueryHandler.ClampSize(request.PageSize),
            request.Status,
            ct);

        return Result<PagedResult<MoveOrderDto>>.Success(paged);
    }
}

public sealed class GetTransactedMoveOrdersQueryHandler
    : IRequestHandler<GetTransactedMoveOrdersQuery, Result<PagedResult<TransactedMoveOrderDto>>>
{
    private readonly IOrderReader _reader;

    public GetTransactedMoveOrdersQueryHandler(IOrderReader reader) => _reader = reader;

    public async Task<Result<PagedResult<TransactedMoveOrderDto>>> Handle(
        GetTransactedMoveOrdersQuery request, CancellationToken ct)
    {
        var paged = await _reader.GetTransactedPagedAsync(
            GetOrdersQueryHandler.Clamp(request.Page),
            GetOrdersQueryHandler.ClampSize(request.PageSize),
            ct);

        return Result<PagedResult<TransactedMoveOrderDto>>.Success(paged);
    }
}
