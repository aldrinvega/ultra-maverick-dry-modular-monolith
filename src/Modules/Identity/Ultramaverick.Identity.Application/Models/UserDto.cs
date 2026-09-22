namespace Ultramaverick.Identity.Application.Models
{
    public sealed record UserDto(
        int Id,
        string FullName,
        string UserName,
        int RoleId,
        string RoleName,
        int DepartmentId,
        string DepartmentName,
        bool IsActive);
}
