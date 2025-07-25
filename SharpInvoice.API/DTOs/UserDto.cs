namespace SharpInvoice.API.Dtos;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public sealed record UserDto
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// The user's phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Indicates whether the user's email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// The user's role.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// The date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the user was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}