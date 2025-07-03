namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for updating business details.
/// </summary>
/// <param name="Name">The updated name of the business.</param>
/// <param name="Email">The updated contact email for the business.</param>
/// <param name="PhoneNumber">The updated contact phone number for the business.</param>
/// <param name="Website">The updated website URL of the business.</param>
/// <param name="Address">The updated street address of the business.</param>
/// <param name="City">The updated city where the business is located.</param>
/// <param name="State">The updated state or province where the business is located.</param>
/// <param name="ZipCode">The updated postal or ZIP code for the business address.</param>
/// <param name="Country">The updated country where the business is located.</param>
/// <param name="LogoUrl">The updated URL to the business logo image.</param>
/// <param name="ThemeSettings">Custom theme settings for the business (colors, fonts, etc.).</param>
public record UpdateBusinessDetailsDto(
    [property: Required, MinLength(2), MaxLength(100)] string Name,
    [property: EmailAddress] string? Email,
    [property: Phone] string? PhoneNumber,
    [property: Url] string? Website,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    [property: Url] string? LogoUrl,
    object? ThemeSettings
); 