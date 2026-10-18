namespace Ultramaverick.Quality.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Quality module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
