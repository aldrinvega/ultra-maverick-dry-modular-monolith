using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IReferenceDataService
{
    Task<PagedResult<LookupDto>> ListAsync(
        string entity, int page, int pageSize, string? search, bool? isActive, CancellationToken ct);

    Task<LookupDto?> GetAsync(string entity, int id, CancellationToken ct);

    Task<Result<int>> CreateAsync(string entity, string name, string? code, CancellationToken ct);

    Task<Result> UpdateAsync(string entity, int id, string name, string? code, CancellationToken ct);

    Task<Result> SetActiveAsync(string entity, int id, bool isActive, CancellationToken ct);
}
