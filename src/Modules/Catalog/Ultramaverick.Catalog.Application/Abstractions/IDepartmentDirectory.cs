namespace Ultramaverick.Catalog.Application.Abstractions;

/// <summary>
/// Cross-context port into Identity's department data. Catalog persists DepartmentId as a
/// scalar with no foreign key; the composition root bridges this port to Identity's query
/// surface so the two modules stay independent.
/// </summary>
public interface IDepartmentDirectory
{
    Task<bool> ExistsAsync(int departmentId, CancellationToken ct);
}