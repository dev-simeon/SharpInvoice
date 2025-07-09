namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using SharpInvoice.Core.Domain.Shared;

public sealed class Business : AuditableEntity<Guid>
{       
    private Business(Guid id, string name, Guid ownerId, string country) : base(id)
    {
        Name = name;
        OwnerId = ownerId;
        Country = country;
        IsActive = true;
        ThemeSettings = "{}";
    }

    public static Business Create(string name, Guid ownerId, string country)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        return new(Guid.NewGuid(), name, ownerId, country);
    }

    public void UpdateDetails(string name, string? email, string? phone, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));

        Name = name;
        Email = email;
        PhoneNumber = phone;
        Website = website;
    }

    public void UpdateAddress(string? address, string? city, string? state, string? zip, string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        Address = address;
        City = city;
        State = state;
        ZipCode = zip;
        Country = country;
    }

    public void UpdateBranding(string? logoUrl, string themeSettingsJson)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(themeSettingsJson);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ArgumentException($"Theme settings must be a valid JSON string. Details: {ex.Message}", nameof(themeSettingsJson));
        }

        LogoUrl = logoUrl;
        ThemeSettings = themeSettingsJson;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

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

    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public Guid OwnerId { get; private init; }
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

    public readonly List<TeamMember> _teamMembers = [];

    public readonly List<Invitation> _invitations = [];


    // EF Core
    private Business()
    {
        Name = string.Empty;
        Country = string.Empty;
        ThemeSettings = "{}";
    }
}