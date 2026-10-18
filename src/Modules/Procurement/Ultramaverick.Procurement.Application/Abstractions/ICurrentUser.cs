namespace Ultramaverick.Procurement.Application.Abstractions;

/// <summary>The authenticated caller, as seen by the Procurement module.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
