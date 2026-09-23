namespace Ultramaverick.Identity.Application.Models
{
    public sealed record MainMenuDto(int Id, string Name, string Path, int SortOrder, bool IsActive);
}
