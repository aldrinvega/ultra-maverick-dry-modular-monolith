using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id, CancellationToken ct);
    Task<Item?> GetByCodeAsync(string itemCode, CancellationToken ct);
    Task<IReadOnlyList<Item>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(Item item, CancellationToken ct);
}