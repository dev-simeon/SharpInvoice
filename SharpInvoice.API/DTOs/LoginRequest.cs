using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// User login request data
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// User's registered email address
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; init; }

    /// <summary>
    /// User's password
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required]
    [StringLength(100)]
    public required string Password { get; init; }
} 