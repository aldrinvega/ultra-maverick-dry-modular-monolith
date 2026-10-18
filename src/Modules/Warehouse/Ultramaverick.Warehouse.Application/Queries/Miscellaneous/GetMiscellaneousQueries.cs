using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;

namespace Ultramaverick.Warehouse.Application.Queries.Miscellaneous;

public sealed record MiscellaneousReceiptDto(
    int Id,
    int WarehouseId,
    string? RefNo,
    DateTime ReceivedAtUtc,
    string? Remarks,
    int? ReceivedByUserId,
    bool IsActive);

public sealed record MiscellaneousIssueDto(
    int Id,
    int WarehouseId,
    string? RefNo,
    DateTime IssuedAtUtc,
    string? Remarks,
    int? IssuedByUserId,
    bool IsActive);

public sealed record GetMiscellaneousReceiptsQuery(int Page, int PageSize)
    : IRequest<PagedResult<MiscellaneousReceiptDto>>;

public sealed record GetMiscellaneousIssuesQuery(int Page, int PageSize)
    : IRequest<PagedResult<MiscellaneousIssueDto>>;

public sealed class GetMiscellaneousReceiptsQueryHandler
    : IRequestHandler<GetMiscellaneousReceiptsQuery, PagedResult<MiscellaneousReceiptDto>>
{
    private readonly IMiscellaneousReader _reader;

    public GetMiscellaneousReceiptsQueryHandler(IMiscellaneousReader reader) => _reader = reader;

    public Task<PagedResult<MiscellaneousReceiptDto>> Handle(
        GetMiscellaneousReceiptsQuery request, CancellationToken ct)
        => _reader.GetReceiptsPagedAsync(Page(request.Page), PageSize(request.PageSize), ct);

    internal static int Page(int page) => page < 1 ? 1 : page;

    internal static int PageSize(int pageSize) => pageSize < 1 ? 20 : Math.Min(pageSize, 100);
}

public sealed class GetMiscellaneousIssuesQueryHandler
    : IRequestHandler<GetMiscellaneousIssuesQuery, PagedResult<MiscellaneousIssueDto>>
{
    private readonly IMiscellaneousReader _reader;

    public GetMiscellaneousIssuesQueryHandler(IMiscellaneousReader reader) => _reader = reader;

    public Task<PagedResult<MiscellaneousIssueDto>> Handle(
        GetMiscellaneousIssuesQuery request, CancellationToken ct)
        => _reader.GetIssuesPagedAsync(
            GetMiscellaneousReceiptsQueryHandler.Page(request.Page),
            GetMiscellaneousReceiptsQueryHandler.PageSize(request.PageSize),
            ct);
}
