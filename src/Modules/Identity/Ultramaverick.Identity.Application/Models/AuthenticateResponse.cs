namespace Ultramaverick.Identity.Application.Models;
public sealed record AuthenticateResponse(


        int Id,
        string FullName,
        string UserName,
        string Role,
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken
    );
