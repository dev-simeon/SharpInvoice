namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents a business entity returned by the API.
/// </summary>
public class BusinessDto
{
    /// <summary>
    /// The unique identifier for the business.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the business.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The street address of the business.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The city where the business is located.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// The state or province where the business is located.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The country where the business is located.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// The postal or ZIP code.
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// The contact phone number for the business.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The contact email for the business.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The business's website URL.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// The date and time when the business was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
} 