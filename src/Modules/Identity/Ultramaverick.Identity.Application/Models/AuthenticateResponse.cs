namespace Ultramaverick.Identity.Application.Models;

public sealed record AuthenticateResponse(
    int Id,
    string FullName,
    string UserName,
    int RoleId,
    string RoleName,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken);
