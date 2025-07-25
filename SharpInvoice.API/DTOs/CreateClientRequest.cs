using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents the data required to create a new client.
/// </summary>
public class CreateClientRequest
{
    [Required]
    public string BusinessId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }
} 