using Ultramaverick.Reporting.Application.Queries.InventoryPositions;

namespace Ultramaverick.Reporting.Application.Abstractions;

public interface IInventoryPositionReader
{
    Task<IReadOnlyList<InventoryPositionDto>> GetAsync(int? warehouseId, CancellationToken ct);
}
