using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Request to enable or disable two-factor authentication
/// </summary>
public sealed record EnableTwoFactorRequest
{
    /// <summary>
    /// Whether to enable or disable 2FA
    /// </summary>
    [Required]
    public bool Enable { get; init; }
}

/// <summary>
/// Request to verify two-factor authentication code
/// </summary>
public sealed record VerifyTwoFactorRequest
{
    /// <summary>
    /// The user's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    /// The 6-digit verification code
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be exactly 6 digits")]
    public required string Code { get; init; }
}

/// <summary>
/// Response for two-factor authentication status
/// </summary>
public sealed record TwoFactorStatusResponse
{
    /// <summary>
    /// Whether 2FA is enabled for the user
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Message describing the current status
    /// </summary>
    public required string Message { get; init; }
}