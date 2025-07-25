using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Request model for resetting a user's password.
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    /// The password reset token.
    /// </summary>
    [Required]
    public required string Token { get; init; }

    /// <summary>
    /// The new password.
    /// </summary>
    [Required]
    [MinLength(8)]
    public required string NewPassword { get; init; }
}