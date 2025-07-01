namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

public record UpdateBusinessDetailsDto(
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
    object? ThemeSettings
); 