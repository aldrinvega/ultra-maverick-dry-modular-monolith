using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface ILotNameRepository
{
    Task<LotName?> GetByIdAsync(int id, CancellationToken ct);
    Task<LotName?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<LotName>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(LotName lotName, CancellationToken ct);
}