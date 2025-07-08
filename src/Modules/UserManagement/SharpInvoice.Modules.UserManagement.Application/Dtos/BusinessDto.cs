namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

/// <summary>
/// Represents a business in the system.
/// </summary>
/// <param name="Id">The unique identifier for the business.</param>
/// <param name="Name">The name of the business.</param>
public record BusinessDto(Guid Id, string Name);