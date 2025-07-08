namespace SharpInvoice.Modules.Payments.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PaymentMethod { Stripe, BankTransfer, Cash }
public sealed class Transaction : AuditableEntity<Guid>
{
    [Required] public Guid InvoiceId { get; private init; }
    [Required][Column(TypeName = "decimal(18, 2)")] public decimal Amount { get; private init; }
    [Required] public DateTime TransactionDate { get; private init; }
    [Required] public PaymentMethod PaymentMethod { get; private init; }
    [MaxLength(256)] public string? ExternalTransactionId { get; private init; }
    public string? Notes { get; private set; }

    private Transaction(Guid id, Guid invoiceId, decimal amount, DateTime date, PaymentMethod method, string? externalId) : base(id)
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

    private Transaction() { } // EF Core
}