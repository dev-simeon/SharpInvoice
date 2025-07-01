namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

public record UpdateProfileDto(
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string? PhoneNumber
);