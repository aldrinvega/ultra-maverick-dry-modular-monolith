namespace Ultramaverick.Manufacturing.Application.Abstractions;

/// <summary>
/// In-module existence check for the production plan a request points at. Keeping this inside
/// Manufacturing avoids a foreign-key violation surfacing as an unhandled 500.
/// </summary>
public interface IProductionPlanRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct);
}
