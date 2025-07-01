namespace SharpInvoice.Modules.Invoicing.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using SharpInvoice.Modules.Payments.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum InvoiceStatus { Draft, Sent, Paid, Overdue, Void }
public sealed class Invoice : AuditableEntity<Guid>
{
    [Required] public Guid BusinessId { get; private init; }
    [Required] public Guid ClientId { get; private init; }
    public Client Client { get; private init; } = null!;
    [Required][MaxLength(50)] public string InvoiceNumber { get; private set; }
    [Required] public DateTime IssueDate { get; private set; }
    [Required] public DateTime DueDate { get; private set; }
    [Required][Column(TypeName = "decimal(18, 2)")] public decimal SubTotal { get; private set; }
    [Column(TypeName = "decimal(18, 2)")] public decimal Tax { get; private set; }
    [Required][Column(TypeName = "decimal(18, 2)")] public decimal Total { get; private set; }
    [Column(TypeName = "decimal(18, 2)")] public decimal AmountPaid { get; private set; }
    [Required][MaxLength(3)] public string Currency { get; private set; }
    [Required] public InvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }

    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Invoice(Guid id, Guid businessId, Guid clientId, string invoiceNumber, string currency, string creatorId) : base(id)
    {
        BusinessId = businessId; ClientId = clientId; InvoiceNumber = invoiceNumber; Currency = currency;
        Status = InvoiceStatus.Draft; IssueDate = DateTime.UtcNow; DueDate = DateTime.UtcNow.AddDays(30);
        CreatedBy = creatorId; // Tracks the user who issued the invoice
    }

    public static Invoice Create(Guid businessId, Guid clientId, string invoiceNumber, string currency, string creatorId, bool businessIsActive)
    {
        if (!businessIsActive)
        {
            throw new InvalidOperationException("Invoices cannot be added to an inactive business.");
        }
        return new(Guid.NewGuid(), businessId, clientId, invoiceNumber, currency, creatorId);
    }

    public void UpdateDetails(DateTime issueDate, DateTime dueDate, string? notes, string? terms)
    {
        IssueDate = issueDate; DueDate = dueDate; Notes = notes; Terms = terms;
    }

    public void AddItem(string description, decimal quantity, decimal unitPrice, string? unit)
    {
        if (Status != InvoiceStatus.Draft) throw new InvalidOperationException("Cannot modify an invoice that is not a draft.");
        var item = InvoiceItem.Create(Id, description, quantity, unitPrice, unit);
        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != InvoiceStatus.Draft) throw new InvalidOperationException("Cannot modify an invoice that is not a draft.");
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    public void ApplyPayment(decimal amount, PaymentMethod method, string? externalId)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Void) throw new InvalidOperationException("Cannot apply payment to a paid or voided invoice.");
        var transaction = Transaction.Create(Id, amount, DateTime.UtcNow, method, externalId);
        _transactions.Add(transaction);
        AmountPaid += amount;
        if (AmountPaid >= Total) Status = InvoiceStatus.Paid;
    }

    public void MarkAsSent()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Cannot send an invoice with no items.");
        if (Status == InvoiceStatus.Draft) Status = InvoiceStatus.Sent;
    }

    public void Void()
    {
        if (Status != InvoiceStatus.Paid) Status = InvoiceStatus.Void;
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.Total);
        Tax = SubTotal * 0.1m;
        Total = SubTotal + Tax;
    }

    private Invoice() { InvoiceNumber = string.Empty; Currency = string.Empty; Client = null!; } // EF Core
}