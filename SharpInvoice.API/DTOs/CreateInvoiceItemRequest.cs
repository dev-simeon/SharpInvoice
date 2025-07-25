using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

public class CreateInvoiceItemRequest
{
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1000000)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal UnitPrice { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }
} 