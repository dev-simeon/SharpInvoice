namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

/// <summary>
/// Represents detailed information about a business.
/// </summary>
/// <param name="Id">The unique identifier for the business.</param>
/// <param name="Name">The name of the business.</param>
/// <param name="Email">The contact email for the business.</param>
/// <param name="PhoneNumber">The contact phone number for the business.</param>
/// <param name="Website">The website URL of the business.</param>
/// <param name="Address">The street address of the business.</param>
/// <param name="City">The city where the business is located.</param>
/// <param name="State">The state or province where the business is located.</param>
/// <param name="ZipCode">The postal or ZIP code for the business address.</param>
/// <param name="Country">The country where the business is located.</param>
/// <param name="LogoUrl">The URL to the business logo image.</param>
/// <param name="IsActive">Indicates whether the business is currently active.</param>
public record BusinessDetailsDto(
    Guid Id,
    string Name,
    string? Email,
    string? PhoneNumber,
    string? Website,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? LogoUrl,
    bool IsActive
);