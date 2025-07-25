using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents the data required to create a new business.
/// </summary>
public class CreateBusinessRequest
{
    /// <summary>
    /// The name of the business.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The street address of the business.
    /// </summary>
    [StringLength(255, ErrorMessage = "Address cannot be longer than 255 characters.")]
    public string? Address { get; set; }

    /// <summary>
    /// The city where the business is located.
    /// </summary>
    [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters.")]
    public string? City { get; set; }

    /// <summary>
    /// The state or province where the business is located.
    /// </summary>
    [StringLength(100, ErrorMessage = "State cannot be longer than 100 characters.")]
    public string? State { get; set; }

    /// <summary>
    /// The country where the business is located.
    /// </summary>
    [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters.")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// The postal or ZIP code.
    /// </summary>
    [StringLength(20, ErrorMessage = "Zip code cannot be longer than 20 characters.")]
    public string? ZipCode { get; set; }

    /// <summary>
    /// The contact phone number for the business.
    /// </summary>
    [Phone]
    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The contact email for the business.
    /// </summary>
    [EmailAddress]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public string? Email { get; set; }

    /// <summary>
    /// The business's website URL.
    /// </summary>
    [Url]
    [StringLength(100, ErrorMessage = "Website URL cannot be longer than 100 characters.")]
    public string? Website { get; set; }
} 