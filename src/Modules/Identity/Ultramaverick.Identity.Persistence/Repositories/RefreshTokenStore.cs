using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Persistence.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories
{
    public sealed class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly IdentityDbContext _context;
        public RefreshTokenStore(IdentityDbContext context) => _context = context;

        public async Task<int?> ConsumeAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == Hash(token), ct);
            if(entity is null || !entity.IsActive(DateTime.UtcNow)) return null;
            entity.MarkConsumed();
            await _context.SaveChangesAsync(ct);
            return entity.UserId;
        }

        public async Task<string> IssueAsync(int userId, string token, DateTime expiresAtUtc, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            _context.RefreshTokens.Add(RefreshToken.Create(userId, Hash(token), expiresAtUtc));

            await _context.SaveChangesAsync(ct);
            return token;
        }

        public async Task RevokeAllForUserAsync(int userId, CancellationToken ct)
        {
            var tokens = await _context.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ConsumedAtUtc == null).ToListAsync(ct);
                 foreach (var token in tokens) token.Revoke();

            await _context.SaveChangesAsync(ct);
        }

        public async Task RevokeAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == Hash(token), ct);
            if (entity is null || entity.RevokedAtUtc is not null) return;
            entity.Revoke();
            await _context.SaveChangesAsync(ct);
        }

        private static string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    } 
}
