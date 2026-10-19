using MediatR;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Application.Models;

namespace Ultramaverick.Laboratory.Application.Queries.LabTestRequests;

public sealed record LabTestRequestDto(
    int Id,
    int? WarehouseReceiptId,
    string? BatchNo,
    decimal Quantity,
    string? TestType,
    DateTime? DateRequested,
    DateTime? DateNeeded,
    string Status,
    string? Remarks,
    int? RequestedByUserId,
    bool IsActive);

public sealed record GetLabTestRequestsQuery(string? Status, int Page, int PageSize)
    : IRequest<PagedResult<LabTestRequestDto>>;

public sealed record GetLabTestRequestByIdQuery(int Id)
    : IRequest<Result<LabTestRequestDto>>;

public sealed class GetLabTestRequestsQueryHandler
    : IRequestHandler<GetLabTestRequestsQuery, PagedResult<LabTestRequestDto>>
{
    private readonly ILabTestRequestReader _reader;

    public GetLabTestRequestsQueryHandler(ILabTestRequestReader reader) => _reader = reader;

    public Task<PagedResult<LabTestRequestDto>> Handle(GetLabTestRequestsQuery request, CancellationToken ct)
        => _reader.GetPagedAsync(
            request.Status,
            request.Page < 1 ? 1 : request.Page,
            request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100),
            ct);
}

public sealed class GetLabTestRequestByIdQueryHandler
    : IRequestHandler<GetLabTestRequestByIdQuery, Result<LabTestRequestDto>>
{
    private readonly ILabTestRequestReader _reader;

    public GetLabTestRequestByIdQueryHandler(ILabTestRequestReader reader) => _reader = reader;

    public async Task<Result<LabTestRequestDto>> Handle(GetLabTestRequestByIdQuery request, CancellationToken ct)
    {
        if (request.Id <= 0)
            return Result<LabTestRequestDto>.Failure("Lab test request id must be greater than zero.");

        var request2 = await _reader.GetByIdAsync(request.Id, ct);

        return request2 is null
            ? Result<LabTestRequestDto>.Failure("Lab test request not found.")
            : Result<LabTestRequestDto>.Success(request2);
    }
}
