using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token, out string userId);
    string GenerateEmailConfirmationToken();
    string GeneratePasswordResetToken();
} 