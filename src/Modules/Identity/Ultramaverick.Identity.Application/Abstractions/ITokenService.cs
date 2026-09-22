using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface ITokenService
    {
        AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> moduleNames);
        string CreateRefreshToken();
        int RefreshTokenDays { get; }
    }

    public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
}
