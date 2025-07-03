namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the data needed to update a user's profile.
/// </summary>
/// <param name="FirstName">The user's updated first name.</param>
/// <param name="LastName">The user's updated last name.</param>
/// <param name="AvatarUrl">The URL to the user's updated profile picture/avatar, if available.</param>
/// <param name="PhoneNumber">The user's updated contact phone number, if available.</param>
public record UpdateProfileDto(
    [property: Required, MinLength(2), MaxLength(50), Description("The user's first name.")] 
    string FirstName,
    
    [property: Required, MinLength(2), MaxLength(50), Description("The user's last name.")] 
    string LastName,
    
    [property: Url, Description("The URL to the user's profile picture/avatar.")] 
    string? AvatarUrl,
    
    [property: Phone, Description("The user's contact phone number.")] 
    string? PhoneNumber
);