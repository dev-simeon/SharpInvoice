using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Options;

namespace SharpInvoice.Infrastructure.Services;

/// <summary>
/// Implementation of IPasswordHasher using BCrypt.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PasswordHasher"/> class.
/// </remarks>
/// <param name="appSettings">The application settings containing security configurations.</param>
public class PasswordHasher(AppSettings appSettings) : IPasswordHasher
{
    private readonly int _workFactor = appSettings.Security?.BcryptWorkFactor ?? 12;

    /// <summary>
    /// Hashes a password using BCrypt with a secure work factor.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A securely hashed password.</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    /// <summary>
    /// Verifies that a password matches a hashed password.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hashedPassword">The hashed password to verify against.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword), "Hashed password cannot be null or empty");
            
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

