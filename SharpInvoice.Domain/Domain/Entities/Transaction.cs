namespace SharpInvoice.Core.Domain.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using SharpInvoice.Core.Domain.Enums;

public sealed class Transaction : BaseEntity
{
    private Transaction(Guid id, Guid invoiceId, decimal amount, DateTime date, PaymentMethod method, string? externalId) 
    {
        InvoiceId = invoiceId;
        Amount = amount;
        TransactionDate = date;
        PaymentMethod = method;
        ExternalTransactionId = externalId;
    }

    public static Transaction Create(Guid invoiceId, decimal amount, DateTime date, PaymentMethod method, string? externalId)
    {
        return new(Guid.NewGuid(), invoiceId, amount, date, method, externalId);
    }

    public void AddNote(string note) => Notes = note;

    public Guid InvoiceId { get; private init; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; private init; }
    public DateTime TransactionDate { get; private init; }
    public PaymentMethod PaymentMethod { get; private init; }
    public string? ExternalTransactionId { get; private init; }
    public string? Notes { get; private set; }

    private Transaction() { } // EF Core
}