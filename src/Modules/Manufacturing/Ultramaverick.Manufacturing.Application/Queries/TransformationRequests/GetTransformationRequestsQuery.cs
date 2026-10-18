using MediatR;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;

namespace Ultramaverick.Manufacturing.Application.Queries.TransformationRequests;

public sealed record TransformationRequestSummaryDto(
    int Id,
    int ProductionPlanId,
    int WarehouseId,
    string Status,
    bool IsActive,
    int LineCount,
    DateTime CreatedAtUtc);

public sealed record TransformationRequestLineDto(
    int Id,
    int ItemId,
    decimal RequiredQuantity);

public sealed record TransformationRequestPreparationDto(
    int Id,
    int? PreparedByUserId,
    DateTime PreparedAtUtc,
    decimal QuantityNeeded);

public sealed record TransformationRequestRejectDto(
    int Id,
    int ReasonId,
    decimal Quantity,
    DateTime RejectedAtUtc);

public sealed record TransformationRequestDetailDto(
    int Id,
    int ProductionPlanId,
    int WarehouseId,
    string Status,
    bool IsActive,
    DateTime CreatedAtUtc,
    IReadOnlyList<TransformationRequestLineDto> Lines,
    IReadOnlyList<TransformationRequestPreparationDto> Preparations,
    IReadOnlyList<TransformationRequestRejectDto> Rejects);

public sealed record GetTransformationRequestsQuery(string? Status, int Page, int PageSize)
    : IRequest<Result<PagedResult<TransformationRequestSummaryDto>>>;

public sealed record GetTransformationRequestByIdQuery(int Id)
    : IRequest<Result<TransformationRequestDetailDto>>;

public sealed class GetTransformationRequestsQueryHandler
    : IRequestHandler<GetTransformationRequestsQuery, Result<PagedResult<TransformationRequestSummaryDto>>>
{
    private const int MaxPageSize = 100;

    private readonly ITransformationRequestReader _reader;

    public GetTransformationRequestsQueryHandler(ITransformationRequestReader reader) => _reader = reader;

    public async Task<Result<PagedResult<TransformationRequestSummaryDto>>> Handle(
        GetTransformationRequestsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);

        var paged = await _reader.GetPagedAsync(request.Status, page, pageSize, ct);
        return Result<PagedResult<TransformationRequestSummaryDto>>.Success(paged);
    }
}

public sealed class GetTransformationRequestByIdQueryHandler
    : IRequestHandler<GetTransformationRequestByIdQuery, Result<TransformationRequestDetailDto>>
{
    private readonly ITransformationRequestReader _reader;

    public GetTransformationRequestByIdQueryHandler(ITransformationRequestReader reader) => _reader = reader;

    public async Task<Result<TransformationRequestDetailDto>> Handle(
        GetTransformationRequestByIdQuery request, CancellationToken ct)
    {
        if (request.Id <= 0)
            return Result<TransformationRequestDetailDto>.Failure("Request id must be greater than zero.");

        var detail = await _reader.GetByIdAsync(request.Id, ct);

        return detail is null
            ? Result<TransformationRequestDetailDto>.Failure("Transformation request not found.")
            : Result<TransformationRequestDetailDto>.Success(detail);
    }
}
