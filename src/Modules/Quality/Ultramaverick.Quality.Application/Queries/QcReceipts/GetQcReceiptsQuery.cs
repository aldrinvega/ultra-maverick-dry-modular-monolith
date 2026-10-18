using MediatR;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Queries.QcReceipts;

public sealed record GetQcReceiptsQuery(int Page, int PageSize, string? Search)
    : IRequest<PagedResult<QcReceiptDto>>;
