using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Persistence.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories
{
    public sealed class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly IdentityDbContext _context;

        public RefreshTokenStore(IdentityDbContext context) => _context = context;

        public async Task<string> IssueAsync(int userId, string token, DateTime expiresAtUtc, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            _context.RefreshTokens.Add(RefreshToken.Create(userId, Hash(token), expiresAtUtc));
            await _context.SaveChangesAsync(ct);

            return token;
        }

        public async Task<int?> ConsumeAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var entity = await FindActiveAsync(token, ct);
            if (entity is null) return null;

            entity.MarkConsumed();
            await _context.SaveChangesAsync(ct);

            return entity.UserId;
        }

        public async Task<int?> RotateAsync(
            string presentedToken, string newToken, DateTime newExpiresAtUtc, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(presentedToken)) return null;
            ArgumentException.ThrowIfNullOrWhiteSpace(newToken);

            var entity = await FindActiveAsync(presentedToken, ct);
            if (entity is null) return null;

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Conditional consume: the database decides the winner, so two concurrent
            // rotations of the same token cannot both succeed.
            var consumed = await _context.RefreshTokens
                .Where(x => x.Id == entity.Id && x.ConsumedAtUtc == null && x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.ConsumedAtUtc, DateTime.UtcNow), ct);

            if (consumed == 0)
            {
                await transaction.RollbackAsync(ct);
                return null;
            }

            _context.RefreshTokens.Add(RefreshToken.Create(entity.UserId, Hash(newToken), newExpiresAtUtc));
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return entity.UserId;
        }

        public async Task RevokeAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            var entity = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == Hash(token), ct);

            if (entity is null || entity.RevokedAtUtc is not null) return;

            entity.Revoke();
            await _context.SaveChangesAsync(ct);
        }

        public async Task RevokeAllForUserAsync(int userId, CancellationToken ct)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ConsumedAtUtc == null)
                .ToListAsync(ct);

            foreach (var token in tokens)
                token.Revoke();

            await _context.SaveChangesAsync(ct);
        }

        private Task<RefreshToken?> FindActiveAsync(string token, CancellationToken ct)
        {
            var hash = Hash(token);
            var now = DateTime.UtcNow;

            return _context.RefreshTokens.FirstOrDefaultAsync(
                x => x.TokenHash == hash
                     && x.ConsumedAtUtc == null
                     && x.RevokedAtUtc == null
                     && x.ExpiresAtUtc > now, ct);
        }

        private static string Hash(string token) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
