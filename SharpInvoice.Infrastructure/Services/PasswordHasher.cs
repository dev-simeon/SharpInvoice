namespace SharpInvoice.Infrastructure.Services;

using BCrypt.Net;
using SharpInvoice.Core.Interfaces.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Verify(password, hash);
    }
}

