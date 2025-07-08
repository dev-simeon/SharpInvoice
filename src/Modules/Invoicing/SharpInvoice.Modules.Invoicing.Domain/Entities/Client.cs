namespace SharpInvoice.Modules.Invoicing.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public sealed class Client : AuditableEntity<Guid>
{
    [Required] public Guid BusinessId { get; private init; }
    [Required][MaxLength(200)] public string Name { get; private set; }
    [EmailAddress][MaxLength(256)] public string? Email { get; private set; }
    [Phone][MaxLength(50)] public string? Phone { get; private set; }
    [MaxLength(255)] public string? Address { get; private set; }
    [MaxLength(100)] public string? Country { get; private set; }
    [MaxLength(10)] public string? Locale { get; private set; }

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

    private Client() { Name = string.Empty; } // EF Core
}