namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

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