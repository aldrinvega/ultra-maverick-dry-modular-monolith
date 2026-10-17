using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IReasonRepository
{
    Task<Reason?> GetByIdAsync(int id, CancellationToken ct);
    Task<Reason?> GetByNameAsync(string name, CancellationToken ct);
    Task<IReadOnlyList<Reason>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Reason reason, CancellationToken ct);
}