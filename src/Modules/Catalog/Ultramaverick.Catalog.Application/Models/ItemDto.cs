namespace Ultramaverick.Catalog.Application.Models;

public sealed record ItemDto(
    int Id,
    string ItemCode,
    string ItemDescription,
    int ItemCategoryId,
    string ItemCategoryName,
    int UomId,
    string UomDescription,
    decimal? BufferLevel,
    bool IsExpirable,
    bool IsActive);