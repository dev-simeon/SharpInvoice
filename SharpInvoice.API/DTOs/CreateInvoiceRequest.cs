using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

public class CreateInvoiceRequest
{
    [Required]
    public string BusinessId { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public DateTime IssueDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? Terms { get; set; }

    public List<CreateInvoiceItemRequest> Items { get; set; } = [];
} 