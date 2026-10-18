namespace Ultramaverick.Orders.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Orders module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
