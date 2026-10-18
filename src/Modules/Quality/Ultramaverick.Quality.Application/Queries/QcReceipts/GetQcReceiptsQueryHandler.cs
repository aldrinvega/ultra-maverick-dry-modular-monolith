using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Queries.QcReceipts;

public sealed class GetQcReceiptsQueryHandler
    : IRequestHandler<GetQcReceiptsQuery, PagedResult<QcReceiptDto>>
{
    private readonly IQualityUnitOfWork _uow;

    public GetQcReceiptsQueryHandler(IQualityUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<QcReceiptDto>> Handle(GetQcReceiptsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page > 10_000 ? 10_000 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var receipts = await _uow.QcReceipts.ListAsync(request.Search, skip, pageSize, ct);
        var total = await _uow.QcReceipts.CountAsync(request.Search, ct);

        var dtos = receipts.Select(QcReceiptDto.From).ToList();

        return new PagedResult<QcReceiptDto>(dtos, page, pageSize, total);
    }
}
