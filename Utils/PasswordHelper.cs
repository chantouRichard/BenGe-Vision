using Microsoft.AspNetCore.Identity;

namespace picture_backend.Utils
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<string> hasher = new();

        public static string HashPassword(string password)
        {
            return hasher.HashPassword(null, password);
        }

        public static bool VerifyPassword(string hashedPassword, string password)
        {
            return hasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;
        }
    }
}
