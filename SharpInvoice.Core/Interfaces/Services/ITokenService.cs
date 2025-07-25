using System.Security.Claims;

namespace SharpInvoice.Core.Interfaces.Services;

/// <summary>
/// Interface for token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateAccessToken(IEnumerable<Claim> claims);
    
    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a token and extracts the principal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The ClaimsPrincipal if valid; otherwise, null.</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    
    /// <summary>
    /// Blacklists a token to prevent its further use.
    /// </summary>
    /// <param name="token">The token to blacklist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BlacklistTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a token has been blacklisted.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if blacklisted, false otherwise.</returns>
    Task<bool> IsTokenBlacklisted(string token, CancellationToken cancellationToken = default);
} 