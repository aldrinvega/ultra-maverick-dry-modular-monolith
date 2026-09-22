using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IRefreshTokenStore
    {
        Task<string> IssueAsync(int userId, string token, DateTime expiresAtUtc,CancellationToken ct);
        Task<int?> ConsumeAsync(string token, CancellationToken ct);
        Task RevokeAsync(string token, CancellationToken ct);
        Task RevokeAllForUserAsync(int userId, CancellationToken ct);
    }
}
