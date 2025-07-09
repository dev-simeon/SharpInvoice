namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Invitation : BaseEntity
{
   private Invitation(Guid id, Guid businessId, string email, Guid roleId, string token, DateTime expiry) 
    {
        BusinessId = businessId;
        InvitedUserEmail = email;
        RoleId = roleId;
        Token = token;
        ExpiryDate = expiry;
        Status = InvitationStatus.Pending;
    }

    public static Invitation Create(Guid businessId, string email, Guid roleId, int validityInHours, ICollection<TeamMember> existingMembers)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Invited email cannot be empty.", nameof(email));

        if (existingMembers.Any(m => m.User.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A team member with this email already exists.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddHours(validityInHours);

        return new Invitation(Guid.NewGuid(), businessId, email, roleId, token, expiry);
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

    public Guid BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public string InvitedUserEmail { get; private init; }
    public Guid RoleId { get; private init; }
    public Role Role { get; private init; } = null!;
    public string Token { get; private init; }
    public DateTime ExpiryDate { get; private init; }
    public InvitationStatus Status { get; private set; }


    private Invitation() { InvitedUserEmail = string.Empty; Token = string.Empty; Role = null!; Business = null!; }
}