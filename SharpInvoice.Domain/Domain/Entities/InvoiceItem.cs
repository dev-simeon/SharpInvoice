namespace SharpInvoice.Core.Domain.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class InvoiceItem : BaseEntity
{
    private InvoiceItem(Guid invoiceId, string description, decimal quantity, decimal unitPrice, string? unit) 
    {
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Total = quantity * unitPrice;
        Unit = unit;
    }

    internal static InvoiceItem Create(Guid invoiceId, string description, decimal quantity, decimal unitPrice, string? unit)
        => new(invoiceId, description, quantity, unitPrice, unit);

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

    private InvoiceItem() { Description = string.Empty; }
}