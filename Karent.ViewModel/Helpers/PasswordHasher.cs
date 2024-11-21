using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Karent.ViewModel.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int HashSize = 32; // 256-bit
        private const int Iterations = 10000; // Adjust for security and performance

        /// <summary>
        /// Hashes a password with a unique salt using PBKDF2.
        /// </summary>
        public static string HashPassword(string password)
        {
            // Generate a salt
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash
                var hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Convert to Base64 for storage
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Extract salt and hash from the stored hash
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Compute the hash of the entered password
            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var enteredHash = pbkdf2.GetBytes(HashSize);

                // Extract the stored hash
                var storedHashBytes = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHashBytes, 0, HashSize);

                // Compare the hashes securely
                return CryptographicOperations.FixedTimeEquals(enteredHash, storedHashBytes);
            }
        }
    }
}
