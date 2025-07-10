namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Invitation : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public string InvitedUserEmail { get; private init; }
    public string RoleId { get; private init; }
    public Role Role { get; private init; } = null!;
    public string Token { get; private init; }
    public DateTime ExpiryDate { get; private init; }
    public InvitationStatus Status { get; private set; }

    // Constructor
    public Invitation(string businessId, string email, string roleId, int validityInHours)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Id = KeyGenerator.Generate("invite", email);
        BusinessId = businessId;
        InvitedUserEmail = email;
        RoleId = roleId;
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        ExpiryDate = DateTime.UtcNow.AddHours(validityInHours);
        Status = InvitationStatus.Pending;
    }

    // Methods
    public static void ValidateInvitation(string email, ICollection<TeamMember> existingMembers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (existingMembers.Any(m => m.User.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A team member with this email already exists.");
    }

    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only a pending invitation can be accepted.");

        if (DateTime.UtcNow > ExpiryDate)
            throw new InvalidOperationException("This invitation has expired.");

        Status = InvitationStatus.Accepted;
    }

    public void Expire()
    {
        if (Status == InvitationStatus.Pending)
            Status = InvitationStatus.Expired;
    }
}