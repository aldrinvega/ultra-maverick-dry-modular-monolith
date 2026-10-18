namespace Ultramaverick.Laboratory.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Laboratory module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
