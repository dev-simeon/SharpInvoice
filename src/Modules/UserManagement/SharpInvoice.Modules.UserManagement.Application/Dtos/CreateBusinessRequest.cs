namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the request to create a new business.
/// </summary>
/// <param name="Name">The name of the business.</param>
/// <param name="Email">The contact email for the business.</param>
/// <param name="Country">The country where the business is located.</param>
public record CreateBusinessRequest(
    [property: Required, MinLength(2), MaxLength(100)] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Required, StringLength(2, MinimumLength = 2)] string Country
); 