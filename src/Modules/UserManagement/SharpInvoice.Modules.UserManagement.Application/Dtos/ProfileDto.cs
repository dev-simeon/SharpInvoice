namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;

/// <summary>
/// Represents a user's profile information.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="AvatarUrl">The URL to the user's profile picture/avatar, if available.</param>
/// <param name="PhoneNumber">The user's contact phone number, if available.</param>
public record ProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    string? PhoneNumber
);