namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface ICurrentUser
    {
        int? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
    }
}
