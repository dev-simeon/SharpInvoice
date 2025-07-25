using SharpInvoice.Core.Enums;

namespace SharpInvoice.API.Dtos;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string BusinessId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
} 