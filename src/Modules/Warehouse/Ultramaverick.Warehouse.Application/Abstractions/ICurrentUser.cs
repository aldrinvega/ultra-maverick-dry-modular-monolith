namespace Ultramaverick.Warehouse.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Warehouse module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
