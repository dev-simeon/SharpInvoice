namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Transaction : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string InvoiceId { get; private init; }
    public Invoice Invoice { get; private init; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; private init; }

    public DateTime TransactionDate { get; private init; }
    public PaymentMethod PaymentMethod { get; private init; }
    public string? ExternalTransactionId { get; private init; }
    public string? Notes { get; private set; }

    // Constructor
    public Transaction(string invoiceId, decimal amount, DateTime date, PaymentMethod method, string? externalId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);

        Id = KeyGenerator.Generate("txn");
        InvoiceId = invoiceId;
        Amount = amount;
        TransactionDate = date;
        PaymentMethod = method;
        ExternalTransactionId = externalId;
    }

    // Methods
    public void AddNote(string note) => Notes = note;
    
    /// <summary>
    /// Overrides the Delete method to prevent transactions from being deleted.
    /// Transactions are immutable financial records and should never be deleted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown when attempting to delete a transaction</exception>
    public new void Delete()
    {
        throw new InvalidOperationException("Transactions cannot be deleted as they are permanent financial records.");
    }
}