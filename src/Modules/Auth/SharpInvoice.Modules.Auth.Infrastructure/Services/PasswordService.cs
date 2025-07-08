using Microsoft.Extensions.Logging;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SharpInvoice.Modules.Auth.Infrastructure.Services
{
    /// <summary>
    /// Service that handles password operations with enhanced security using BCrypt and a pepper
    /// </summary>
    public class PasswordService(AppSettings appSettings, ILogger<PasswordService> logger) : IPasswordService
    {
        private readonly string _pepper = appSettings.Security?.PasswordPepper ??
                throw new InvalidOperationException("Password pepper is not configured in AppSettings");

        /// <summary>
        /// Hashes a password with BCrypt and additional pepper
        /// </summary>
        /// <param name="password">The plain text password</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Apply the pepper by combining it with the password
                string pepperedPassword = ApplyPepper(password);

                // Use BCrypt to hash the peppered password
                return BCrypt.Net.BCrypt.HashPassword(pepperedPassword, workFactor: 12);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while hashing password");
                throw new InvalidOperationException("Could not hash password", ex);
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">The plain text password</param>
        /// <param name="hash">The stored hash</param>
        /// <returns>True if the password matches, false otherwise</returns>
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Apply the pepper
                string pepperedPassword = ApplyPepper(password);

                // Verify the password using BCrypt
                return BCrypt.Net.BCrypt.Verify(pepperedPassword, hash);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while verifying password");
                return false;
            }
        }

        /// <summary>
        /// Applies the application-wide pepper to the password
        /// </summary>
        private string ApplyPepper(string password)
        {
            // Combine the password with the pepper in a deterministic way
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_pepper));
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var pepperedBytes = hmac.ComputeHash(passwordBytes);

            // Convert to Base64 string to use as input for BCrypt
            return Convert.ToBase64String(pepperedBytes);
        }
    }
}