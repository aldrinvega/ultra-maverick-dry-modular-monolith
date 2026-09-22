namespace Ultramaverick.Identity.Domain.ValueObjects
{
    public sealed class PasswordHash
    {
        public string Value { get; }

        private PasswordHash(string value) => Value = value;

        public static PasswordHash FromEncoded(string encoded)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(encoded);
            return new PasswordHash(encoded);
        }
    }
}
