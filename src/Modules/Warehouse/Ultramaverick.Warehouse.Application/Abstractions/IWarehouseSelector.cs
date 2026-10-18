namespace Ultramaverick.Warehouse.Application.Abstractions;

/// <summary>Picks the warehouse a cross-module receipt should land in (default-warehouse rule).</summary>
public interface IWarehouseSelector
{
    /// <summary>Returns the lowest-id active warehouse id; throws when none is configured.</summary>
    Task<int> DefaultForItemAsync(int itemId, CancellationToken ct);
}
