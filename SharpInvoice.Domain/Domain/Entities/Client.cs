namespace SharpInvoice.Core.Domain.Entities;
using System.Collections.Generic;
using SharpInvoice.Core.Domain.Shared;

public sealed class Client : AuditableEntity<Guid>
{
    public Guid BusinessId { get; private init; }
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? Country { get; private set; }
    public string? Locale { get; private set; }

    private readonly List<Invoice> _invoices = []; 
    public IReadOnlyCollection<Invoice> Invoices => _invoices.AsReadOnly();

    private Client(Guid id, Guid businessId, string name) : base(id)
    {
        BusinessId = businessId;
        Name = name;
    }

    public static Client Create(Guid businessId, string name, string? email, string? phone)
    {
        var client = new Client(Guid.NewGuid(), businessId, name);
        client.UpdateContactInfo(email, phone);
        return client;
    }

    public void UpdateName(string newName) => Name = newName;

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

    private Client()
    {
        Name = string.Empty;
    } // EF Core
}