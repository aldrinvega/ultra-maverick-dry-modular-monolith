using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

public interface IFormulaRepository
{
    Task<FormulaHeader?> GetByIdAsync(int id, CancellationToken ct);
    Task<FormulaHeader?> GetByItemAndVersionAsync(int itemId, int version, CancellationToken ct);
    Task<IReadOnlyList<FormulaHeader>> ListAsync(int? itemId, bool? isActive, int skip, int take, CancellationToken ct);
    Task<int> CountAsync(int? itemId, bool? isActive, CancellationToken ct);
    Task AddAsync(FormulaHeader formula, CancellationToken ct);
}