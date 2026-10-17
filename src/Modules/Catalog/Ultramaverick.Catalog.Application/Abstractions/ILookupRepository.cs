namespace Ultramaverick.Catalog.Application.Abstractions;

/// <summary>
/// Generic access to the simple code/name reference tables (item categories, locations,
/// account titles, lot categories, product types). Every member reads the conventional
/// Code/Name/IsActive properties of the entity.
/// </summary>
public interface ILookupRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct);
    Task<T?> GetByNameAsync(string name, CancellationToken ct);
    Task<T?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<T>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
}