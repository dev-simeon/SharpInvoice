namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using SharpInvoice.Core.Domain.Shared;

public sealed class Client : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? Country { get; private set; }
    public string? Locale { get; private set; }

    public ICollection<Invoice> Invoices { get; private set; } = [];

    // Constructor
    public Client(string businessId, string name, string? email = null, string? phone = null, string? address = null, string? country = null, string? locale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
        
        Id = KeyGenerator.Generate("client", name);
        BusinessId = businessId;
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        Country = country;
        Locale = locale;
    }

    // Methods
    public void UpdateName(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName;
    }

    public void UpdateContactInfo(string? email, string? phone)
    {
        Email = email;
        Phone = phone;
    }

    public void UpdateAddress(string? address, string? country, string? locale)
    {
        Address = address;
        Country = country;
        Locale = locale;
    }
    
    /// <summary>
    /// Overrides the Delete method to prevent deletion of clients with associated invoices.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the client has associated invoices</exception>
    public new void Delete()
    {
        if (Invoices != null && Invoices.Any())
        {
            throw new InvalidOperationException("Cannot delete a client that has associated invoices.");
        }
        
        base.Delete(); // Only mark as soft-deleted if no invoices exist
    }
}