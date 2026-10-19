namespace Ultramaverick.Warehouse.Application.Abstractions;

/// <summary>Resolves the unit of measure Catalog holds for an item, since cross-module events
/// carry only item ids.</summary>
public interface IItemUomLookup
{
    Task<int?> GetUomIdAsync(int itemId, CancellationToken ct);
}
