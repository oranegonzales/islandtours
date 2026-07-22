using System;
using System.Globalization;
using System.Security.Cryptography;

namespace invenman.Security
{
    internal static class PasswordSecurity
    {
        private const string Prefix = "$pbkdf2-sha256$";
        private const int Iterations = 600000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required.", nameof(password));
            }

            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, Iterations);
            return Prefix
                + Iterations.ToString(CultureInfo.InvariantCulture)
                + "$" + Convert.ToBase64String(salt)
                + "$" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedValue))
            {
                return false;
            }

            if (!IsPasswordHash(storedValue))
            {
                return ConstantTimeEquals(password, storedValue);
            }

            string[] parts = storedValue.Split('$');
            int iterations;
            if (parts.Length != 5
                || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out iterations)
                || iterations < 10000
                || iterations > 1000000)
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expected = Convert.FromBase64String(parts[4]);
                byte[] actual = Derive(password, salt, iterations);
                return ConstantTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsPasswordHash(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value.StartsWith(Prefix, StringComparison.Ordinal);
        }

        public static bool NeedsRehash(string value)
        {
            if (!IsPasswordHash(value))
            {
                return true;
            }

            string[] parts = value.Split('$');
            int workFactor;
            return parts.Length != 5
                || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out workFactor)
                || workFactor < Iterations;
        }

        public static void RunDummyVerification(string password)
        {
            byte[] fixedSalt =
            {
                0x49, 0x73, 0x6c, 0x61, 0x6e, 0x64, 0x54, 0x6f,
                0x75, 0x72, 0x73, 0x41, 0x75, 0x74, 0x68, 0x21
            };
            Derive(password ?? string.Empty, fixedSalt, Iterations);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                return derive.GetBytes(HashSize);
            }
        }

        private static bool ConstantTimeEquals(string left, string right)
        {
            byte[] leftBytes = System.Text.Encoding.UTF8.GetBytes(left ?? string.Empty);
            byte[] rightBytes = System.Text.Encoding.UTF8.GetBytes(right ?? string.Empty);
            return ConstantTimeEquals(leftBytes, rightBytes);
        }

        private static bool ConstantTimeEquals(byte[] left, byte[] right)
        {
            int difference = left.Length ^ right.Length;
            int length = Math.Max(left.Length, right.Length);
            for (int i = 0; i < length; i++)
            {
                byte leftByte = i < left.Length ? left[i] : (byte)0;
                byte rightByte = i < right.Length ? right[i] : (byte)0;
                difference |= leftByte ^ rightByte;
            }
            return difference == 0;
        }
    }
}
