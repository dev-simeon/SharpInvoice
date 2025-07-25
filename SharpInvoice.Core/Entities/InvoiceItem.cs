namespace SharpInvoice.Core.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using SharpInvoice.Core.Shared;

public sealed class InvoiceItem : BaseEntity
{
    // Properties
    public Guid Id { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; private init; }
    public Guid InvoiceId { get; private init; }
    public Invoice Invoice { get; private init; } = null!;
    public string Description { get; private init; }
    public string? Unit { get; private init; }

    // Constructor
    private InvoiceItem() { }
    public InvoiceItem(Guid invoiceId, string description, decimal quantity, decimal unitPrice, string? unit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Total = quantity * unitPrice;
        Unit = unit;
    }
}