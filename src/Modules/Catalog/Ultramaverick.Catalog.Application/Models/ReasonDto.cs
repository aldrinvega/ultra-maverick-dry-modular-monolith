namespace Ultramaverick.Catalog.Application.Models;

public sealed record ReasonDto(int Id, string Name, int? MenuId, bool IsActive);