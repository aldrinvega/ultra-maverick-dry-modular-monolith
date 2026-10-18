using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Application.Queries.TransformationRequests;

namespace Ultramaverick.Manufacturing.Application.Abstractions;

public interface ITransformationRequestReader
{
    Task<PagedResult<TransformationRequestSummaryDto>> GetPagedAsync(
        string? status, int page, int pageSize, CancellationToken ct);

    Task<TransformationRequestDetailDto?> GetByIdAsync(int id, CancellationToken ct);
}
