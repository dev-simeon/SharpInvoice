using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// User registration request data
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// User's email address (used for login and notifications)
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; init; }

    /// <summary>
    /// User's first name
    /// </summary>
    /// <example>John</example>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; init; }

    /// <summary>
    /// User's last name
    /// </summary>
    /// <example>Doe</example>
    [Required]
    [StringLength(50)]
    public required string LastName { get; init; }

    /// <summary>
    /// Password (must be at least 8 characters with uppercase, lowercase, digit, and special character)
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; init; }

    /// <summary>
    /// Optional phone number in international format
    /// </summary>
    /// <example>+1234567890</example>
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; init; }
} 