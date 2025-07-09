namespace SharpInvoice.Core.Domain.Entities;

using System.Text;
using System.Text.Json;
using SharpInvoice.Core.Domain.Shared;
using SharpInvoice.Modules.UserManagement.Domain.Entities;

public sealed class Business : AuditableEntity<Guid>
{
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

    /// <summary>
    /// Stores theme settings as a JSON string. e.g., { "primary": "#FFFFFF", "secondary": "#000000" }
    /// </summary>
    public string ThemeSettings { get; private set; }

    private readonly List<TeamMember> _teamMembers = [];
    public IReadOnlyCollection<TeamMember> TeamMembers => _teamMembers.AsReadOnly();

    private readonly List<Invitation> _invitations = [];
    public IReadOnlyCollection<Invitation> Invitations => _invitations.AsReadOnly();

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
            throw new BadRequestException("Business name cannot be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new BadRequestException("Country cannot be empty.");

        return new(Guid.NewGuid(), name, ownerId, country);
    }

    public void UpdateDetails(string name, string? email, string? phone, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Business name cannot be empty.");

        Name = name;
        Email = email;
        PhoneNumber = phone;
        Website = website;
    }

    public void UpdateAddress(string? address, string? city, string? state, string? zip, string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new BadRequestException("Country cannot be empty.");

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
            JsonDocument.Parse(themeSettingsJson);
        }
        catch (JsonException ex)
        {
            throw new BadRequestException($"Theme settings must be a valid JSON string. Details: {ex.Message}");
        }

        LogoUrl = logoUrl;
        ThemeSettings = themeSettingsJson;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public string GetFormattedAddress()
    {
        var addressBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(Address)) addressBuilder.AppendLine(Address);
        if (!string.IsNullOrWhiteSpace(City)) addressBuilder.Append($"{City}, ");
        if (!string.IsNullOrWhiteSpace(State)) addressBuilder.Append($"{State} ");
        if (!string.IsNullOrWhiteSpace(ZipCode)) addressBuilder.Append(ZipCode);
        if (!string.IsNullOrWhiteSpace(Country)) addressBuilder.AppendLine($"\n{Country}");
        return addressBuilder.ToString().Trim();
    }

    private Business()
    {
        Name = string.Empty;
        Country = string.Empty;
        ThemeSettings = "{}";
    }
}