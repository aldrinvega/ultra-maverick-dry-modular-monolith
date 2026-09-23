namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IRefreshTokenStore
    {
        Task<string> IssueAsync(int userId, string token, DateTime expiresAtUtc, CancellationToken ct);
        Task<int?> ConsumeAsync(string token, CancellationToken ct);

        /// <summary>
        /// Single-use rotation: consumes the presented token and issues its replacement atomically.
        /// Returns the owner's user id, or null when the token is unknown, expired, or already used.
        /// </summary>
        Task<int?> RotateAsync(string presentedToken, string newToken, DateTime newExpiresAtUtc, CancellationToken ct);

        Task RevokeAsync(string token, CancellationToken ct);
        Task RevokeAllForUserAsync(int userId, CancellationToken ct);

        /// <summary>
        /// Deletes tokens that expired, or were consumed or revoked, before <paramref name="olderThanUtc"/>.
        /// Returns the number of rows removed.
        /// </summary>
        Task<int> PruneAsync(DateTime olderThanUtc, CancellationToken ct);
    }
}
