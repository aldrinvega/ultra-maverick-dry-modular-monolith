namespace Ultramaverick.Catalog.Application.Models;

public sealed record LotNameDto(int Id, string Code, int LotCategoryId, string LotCategoryName, string? SectionName, bool IsActive);