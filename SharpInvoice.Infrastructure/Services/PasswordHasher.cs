using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

