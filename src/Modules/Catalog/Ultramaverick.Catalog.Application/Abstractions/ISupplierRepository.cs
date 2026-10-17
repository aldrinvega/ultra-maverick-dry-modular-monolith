using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct);
    Task<Supplier?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Supplier>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Supplier supplier, CancellationToken ct);
}