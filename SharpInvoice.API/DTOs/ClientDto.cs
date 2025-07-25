namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents a client entity returned by the API.
/// </summary>
public class ClientDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
} 