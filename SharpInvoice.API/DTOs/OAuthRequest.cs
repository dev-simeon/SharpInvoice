using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Request to initiate OAuth authentication
/// </summary>
public sealed record OAuthLoginRequest
{
    /// <summary>
    /// The OAuth provider (google, facebook)
    /// </summary>
    [Required]
    public required string Provider { get; init; }

    /// <summary>
    /// Return URL after OAuth authentication
    /// </summary>
    [Url]
    public string? ReturnUrl { get; init; }
}

/// <summary>
/// Response containing OAuth authentication URL
/// </summary>
public sealed record OAuthUrlResponse
{
    /// <summary>
    /// The OAuth provider URL to redirect to
    /// </summary>
    public required string AuthUrl { get; init; }

    /// <summary>
    /// The state parameter for security validation
    /// </summary>
    public required string State { get; init; }
}

/// <summary>
/// Request to handle OAuth callback
/// </summary>
public sealed record OAuthCallbackRequest
{
    /// <summary>
    /// The OAuth provider (google, facebook)
    /// </summary>
    [Required]
    public required string Provider { get; init; }

    /// <summary>
    /// Authorization code from OAuth provider
    /// </summary>
    [Required]
    public required string Code { get; init; }

    /// <summary>
    /// State parameter for security validation
    /// </summary>
    [Required]
    public required string State { get; init; }

    /// <summary>
    /// Optional error from OAuth provider
    /// </summary>
    public string? Error { get; init; }
}