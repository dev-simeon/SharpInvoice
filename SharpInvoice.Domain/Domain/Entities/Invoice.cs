namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using SharpInvoice.Core.Domain.Shared;
using SharpInvoice.Core.Domain.Enums;

public sealed class Invoice : BaseEntity
{
    private Invoice(Guid businessId, Guid clientId, string invoiceNumber, string currency) 
    {
        BusinessId = businessId;
        ClientId = clientId;
        InvoiceNumber = invoiceNumber;
        Currency = currency;
        Status = InvoiceStatus.Draft;
        IssueDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(30);
    }

    public static Invoice Create(Guid businessId, Guid clientId, string invoiceNumber, string currency, bool businessIsActive)
    {
        if (!businessIsActive)
            throw new InvalidOperationException("Invoices cannot be added to an inactive business.");

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number cannot be empty.", nameof(invoiceNumber));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        return new Invoice(businessId, clientId, invoiceNumber, currency);
    }

    public void UpdateDetails(DateTime issueDate, DateTime dueDate, string? notes, string? terms)
    {
        if (issueDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Issue date cannot be in the past.", nameof(issueDate));
        
        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;
        Terms = terms;
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

    public Guid Id { get; private init; }
    public Guid BusinessId { get; private init; }
    public Guid ClientId { get; private init; }
    public Client Client { get; private init; } = null!;
    public string InvoiceNumber { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal { get; private set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Tax { get; private set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; private set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AmountPaid { get; private set; }
    public string Currency { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }

    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Invoice() { InvoiceNumber = string.Empty; Currency = string.Empty; Client = null!; }
}