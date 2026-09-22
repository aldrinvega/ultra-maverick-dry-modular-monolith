namespace Ultramaverick.Identity.Persistence.Entities
{
    public sealed class RefreshToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime? ConsumedAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        private RefreshToken() { }

        public static RefreshToken Create(int userId, string tokenHash, DateTime expiresAtUtc) => new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        public bool IsActive(DateTime nowUtc) =>
            ConsumedAtUtc is null &&
            RevokedAtUtc is null &&
            ExpiresAtUtc > nowUtc;

        public void MarkConsumed() => ConsumedAtUtc = DateTime.UtcNow;
        public void Revoke() => RevokedAtUtc = DateTime.UtcNow;
    }
}
