using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Options;

namespace SharpInvoice.Infrastructure.Services;

/// <summary>
/// Implementation of ITokenService for JWT token generation and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TokenService"/> class.
/// </remarks>
/// <param name="appSettings">Application settings.</param>
/// <param name="cachingService">Caching service for token blacklisting.</param>
/// <param name="logger">Logger.</param>
public class TokenService(
    AppSettings appSettings,
    ICachingService cachingService,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtSettings _jwtSettings = appSettings.Jwt;
    private readonly ICachingService _cachingService = cachingService;
    private readonly ILogger<TokenService> _logger = logger;


    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtClaims = new List<Claim>(claims);
        
        // Add standard claims if not present
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Iat))
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: jwtClaims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a token and extracts the principal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The ClaimsPrincipal if valid; otherwise, null.</returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            // Check if token is blacklisted
            if (IsTokenBlacklisted(token).GetAwaiter().GetResult())
            {
                _logger.LogWarning("Attempt to use blacklisted token detected");
                return null;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // Allow expired tokens for refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return null;
        }
    }

    /// <summary>
    /// Blacklists a token to prevent its further use.
    /// </summary>
    /// <param name="token">The token to blacklist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BlacklistTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract the token expiration time
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo;
            
            // Calculate how long to cache the blacklisted token (until it expires naturally)
            var timeToLive = expiry - DateTime.UtcNow;
            if (timeToLive <= TimeSpan.Zero)
            {
                // Token already expired, no need to blacklist
                return;
            }

            // Use the token's jti as the key to detect reuse
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                _logger.LogWarning("Token lacks jti claim, using token hash instead");
                jti = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            }

            var cacheKey = $"blacklisted_token:{jti}";
            
            await _cachingService.SetAsync(cacheKey, true, timeToLive, cancellationToken);
            _logger.LogInformation("Token blacklisted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token");
        }
    }

    /// <summary>
    /// Checks if a token has been blacklisted.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if blacklisted, false otherwise.</returns>
    public async Task<bool> IsTokenBlacklisted(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract the jti from the token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                jti = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            }

            var cacheKey = $"blacklisted_token:{jti}";
            var result = await _cachingService.GetAsync<bool>(cacheKey, cancellationToken);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if token is blacklisted");
            return false;
        }
    }
} 