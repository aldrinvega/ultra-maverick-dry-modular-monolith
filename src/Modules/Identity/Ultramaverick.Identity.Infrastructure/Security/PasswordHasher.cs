using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;

namespace Ultramaverick.Identity.Infrastructure.Security
{
    public sealed class PasswordHasher : IPasswordHasher
    {

        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 210_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public string Hash(string plainText)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(
                plainText,
                salt,
                Iterations,
                Algorithm,
                KeySize);
            return string.Join(
                Delimiter,
                "PBKDF2",
                Algorithm.Name,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        public bool Verify(string plainText, string encodedHash)
        {
            if (string.IsNullOrWhiteSpace(plainText) || string.IsNullOrWhiteSpace(encodedHash))
            {
                return false;
            }

            var parts = encodedHash.Split(Delimiter);
            if (parts.Length != 5 || parts[0] != "PBKDF2") return false;
            if (!int.TryParse(parts[2], out var iterations)) return false;

            byte[] salt, expected;

            try
            {
                salt = Convert.FromBase64String(parts[3]);
                expected = Convert.FromBase64String(parts[4]);

            }
            catch (FormatException) { return false; }

            if(salt.Length == 0 || expected.Length == 0) return false;

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                plainText,
                salt,
                iterations,
                new HashAlgorithmName(parts[1]), expected.Length);

            return CryptographicOperations.FixedTimeEquals(expected, actual);

        }
    }
}
