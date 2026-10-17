namespace Ultramaverick.Catalog.Application.Models;

/// <summary>Shared projection for the code/name reference tables.</summary>
public sealed record LookupDto(int Id, string? Code, string Name, bool IsActive);