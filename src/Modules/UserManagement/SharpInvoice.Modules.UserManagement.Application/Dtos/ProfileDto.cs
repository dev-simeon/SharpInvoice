namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;

public record ProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    string? PhoneNumber
);