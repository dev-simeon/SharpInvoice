namespace SharpInvoice.Core.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using SharpInvoice.Core.Domain.Shared;

public sealed class InvoiceItem : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; private init; }
    public string InvoiceId { get; private init; }
    public Invoice Invoice { get; private init; } = null!;
    public string Description { get; private init; }
    public string? Unit { get; private init; }

    // Constructor
    public InvoiceItem(string invoiceId, string description, decimal quantity, decimal unitPrice, string? unit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Id = KeyGenerator.Generate("item", description);
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Total = quantity * unitPrice;
        Unit = unit;
    }
}