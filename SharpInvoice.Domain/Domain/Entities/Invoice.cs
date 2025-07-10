namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Invoice : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public string ClientId { get; private init; }
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

    public ICollection<InvoiceItem> Items { get; private set; } = [];
    public ICollection<Transaction> Transactions { get; private set; } = [];

    // Constructor
    public Invoice(string businessId, string clientId, string invoiceNumber, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        Id = KeyGenerator.Generate("inv", invoiceNumber);
        BusinessId = businessId;
        ClientId = clientId;
        InvoiceNumber = invoiceNumber;
        Currency = currency;
        Status = InvoiceStatus.Draft;
        IssueDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(30);
    }

    // Methods
    public void UpdateDetails(DateTime issueDate, DateTime dueDate, string? notes, string? terms)
    {
        CheckBusinessIsActive();
        
        if (issueDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Issue date cannot be in the past.", nameof(issueDate));

        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;
        Terms = terms;
    }

    public void AddItem(string description, decimal quantity, decimal unitPrice, string? unit)
    {
        CheckBusinessIsActive();
        
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify an invoice that is not a draft.");

        var item = new InvoiceItem(Id, description, quantity, unitPrice, unit);
        Items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(string itemId)
    {
        CheckBusinessIsActive();
        
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify an invoice that is not a draft.");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    public void ApplyPayment(decimal amount, PaymentMethod method, string? externalId)
    {
        CheckBusinessIsActive();
        
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Void)
            throw new InvalidOperationException("Cannot apply payment to a paid or voided invoice.");

        var transaction = new Transaction(Id, amount, DateTime.UtcNow, method, externalId);
        Transactions.Add(transaction);
        AmountPaid += amount;
        if (AmountPaid >= Total)
            Status = InvoiceStatus.Paid;
    }

    public void MarkAsSent()
    {
        CheckBusinessIsActive();
        
        if (Items.Count == 0)
            throw new InvalidOperationException("Cannot send an invoice with no items.");

        if (Status == InvoiceStatus.Draft)
            Status = InvoiceStatus.Sent;
    }

    public void Void()
    {
        CheckBusinessIsActive();
        
        if (Status != InvoiceStatus.Paid)
            Status = InvoiceStatus.Void;
    }

    /// <summary>
    /// Overrides the Delete method to prevent invoices from being deleted.
    /// Invoices are immutable financial records and should never be deleted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown when attempting to delete an invoice</exception>
    public new void Delete()
    {
        throw new InvalidOperationException("Invoices cannot be deleted as they are permanent financial records. Use Void() to mark an invoice as void instead.");
    }

    private void CheckBusinessIsActive()
    {
        if (Business != null && !Business.CanCreateInvoices())
            throw new InvalidOperationException("Cannot modify invoices for an inactive or deleted business.");
    }

    private void RecalculateTotals()
    {
        if (Items == null || !Items.Any())
        {
            SubTotal = 0;
            Tax = 0;
            Total = 0;
            return;
        }
        
        SubTotal = Items.Sum(i => i.Total);
        Tax = SubTotal * 0.1m; // This should be made more flexible later
        Total = SubTotal + Tax;
    }
}