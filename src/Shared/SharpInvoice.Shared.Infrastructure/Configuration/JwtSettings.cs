namespace SharpInvoice.Shared.Infrastructure.Configuration;

/// <summary>
/// Defines the settings for generating JSON Web Tokens (JWT).
/// </summary>
public record JwtSettings
{
    public const string SectionName = "Jwt";
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiresInMinutes { get; init; }
} 