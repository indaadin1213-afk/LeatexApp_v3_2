
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LeatexApp.Models;

namespace LeatexApp.Services
{
    public static class SecurityService
    {
        public static User? Authenticate(string username, string password)
        {
            var users = StorageService.LoadUsers();
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null) return null;
            var hash = HashPassword(password, Convert.FromBase64String(user.Salt));
            return hash == user.PasswordHash ? user : null;
        }

        public static User CreateUser(string username, string password, Uloga role)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var hash = HashPassword(password, saltBytes);
            return new User{ Username=username, PasswordHash=hash, Salt=salt, Role=role };
        }

        private static string HashPassword(string password, byte[] salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var combined = new byte[salt.Length + bytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(bytes, 0, combined, salt.Length, bytes.Length);
            var hash = sha.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
    }
}
