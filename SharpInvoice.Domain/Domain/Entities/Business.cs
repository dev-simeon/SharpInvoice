namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using SharpInvoice.Core.Domain.Shared;

public sealed class Business : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public string OwnerId { get; private init; }
    public User Owner { get; private init; } = null!;
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string Country { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }
    public string ThemeSettings { get; private set; }

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];
    public ICollection<Invitation> Invitations { get; private set; } = [];
    public ICollection<Client> Clients { get; private set; } = [];
    public ICollection<Invoice> Invoices { get; private set; } = [];

    // Constructor
    public Business(string name, string ownerId, string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        Id = KeyGenerator.Generate("biz", name);
        Name = name;
        OwnerId = ownerId;
        Country = country;
        IsActive = true;
        ThemeSettings = "{}";
    }

    // Methods
    public void UpdateDetails(string name, string? email, string? phone, string? website)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Email = email;
        PhoneNumber = phone;
        Website = website;
    }

    public void UpdateAddress(string? address, string? city, string? state, string? zip, string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        Address = address;
        City = city;
        State = state;
        ZipCode = zip;
        Country = country;
    }

    public void UpdateBranding(string? logoUrl, string themeSettingsJson)
    {
        // A simple check for valid JSON can be done here if needed
        LogoUrl = logoUrl;
        ThemeSettings = themeSettingsJson;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Overrides the base Delete method to prevent hard deletion.
    /// Instead, it deactivates the business and marks it as soft-deleted.
    /// When a business is soft-deleted, it will not appear in normal queries,
    /// but the data remains in the database and can be restored.
    /// </summary>
    public new void Delete()
    {
        Deactivate();
        base.Delete(); // Mark as soft-deleted in the base entity
    }

    /// <summary>
    /// Restores a soft-deleted business by clearing the IsDeleted flag
    /// and reactivating the business.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the business is not soft-deleted</exception>
    public void Restore()
    {
        if (!IsDeleted)
        {
            throw new InvalidOperationException("Cannot restore a business that is not soft-deleted.");
        }

        IsDeleted = false;
        DeletedAt = null;
        Activate();
    }

    /// <summary>
    /// Checks if the business can create new invoices
    /// </summary>
    /// <returns>True if the business is active and not deleted</returns>
    public bool CanCreateInvoices() => IsActive && !IsDeleted;

    public string GetFormattedAddress()
    {
        var addressBuilder = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(Address)) addressBuilder.AppendLine(Address);
        if (!string.IsNullOrWhiteSpace(City)) addressBuilder.Append($"{City}, ");
        if (!string.IsNullOrWhiteSpace(State)) addressBuilder.Append($"{State} ");
        if (!string.IsNullOrWhiteSpace(ZipCode)) addressBuilder.Append(ZipCode);
        if (!string.IsNullOrWhiteSpace(Country)) addressBuilder.AppendLine($"\n{Country}");
        return addressBuilder.ToString().Trim();
    }
}