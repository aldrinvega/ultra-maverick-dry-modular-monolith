using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IFarmRepository
{
    Task<Farm?> GetByIdAsync(int id, CancellationToken ct);
    Task<Farm?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Farm>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Farm farm, CancellationToken ct);
}