using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IUomRepository
{
    Task<Uom?> GetByIdAsync(int id, CancellationToken ct);
    Task<Uom?> GetByCodeAsync(string code, CancellationToken ct);
    Task<Uom?> GetByDescriptionAsync(string description, CancellationToken ct);
    Task<IReadOnlyList<Uom>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Uom uom, CancellationToken ct);
}