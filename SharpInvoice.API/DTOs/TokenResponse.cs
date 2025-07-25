namespace SharpInvoice.API.Dtos;

/// <summary>
/// JWT authentication tokens response
/// </summary>
public sealed record TokenResponse
{
    /// <summary>
    /// JWT access token for API authentication (expires in 60 minutes)
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens (expires in 7 days)
    /// </summary>
    /// <example>ef8c0e4c-7b1a-4b8f-9c8a-1234567890ab</example>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    /// <example>Bearer</example>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Access token expiration time in seconds (3600 = 1 hour)
    /// </summary>
    /// <example>3600</example>
    public int ExpiresIn { get; init; } = 3600;
} 