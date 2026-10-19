using Ultramaverick.Laboratory.Application.Models;
using Ultramaverick.Laboratory.Application.Queries.LabTestRequests;

namespace Ultramaverick.Laboratory.Application.Abstractions;

public interface ILabTestRequestReader
{
    Task<PagedResult<LabTestRequestDto>> GetPagedAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<LabTestRequestDto?> GetByIdAsync(int id, CancellationToken ct);
}
