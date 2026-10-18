namespace Ultramaverick.Manufacturing.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Manufacturing module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
