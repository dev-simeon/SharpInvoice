namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Security.Cryptography;
using SharpInvoice.Modules.UserManagement.Domain.Events;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Shared;

public enum InvitationStatus { Pending, Accepted, Expired }
public sealed class Invitation : Entity<Guid>
{
    [Required] public Guid BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    [Required][EmailAddress] public string InvitedUserEmail { get; private init; }
    [Required] public Guid RoleId { get; private init; }
    public Role Role { get; private init; } = null!;
    [Required] public string Token { get; private init; }
    [Required] public DateTime ExpiryDate { get; private init; }
    [Required] public InvitationStatus Status { get; private set; }

    private Invitation(Guid id, Guid businessId, string email, Guid roleId, string token, DateTime expiry) : base(id)
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
            throw new BadRequestException("Invited email cannot be empty.");

        if (existingMembers.Any(m => m.User.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException("A team member with this email already exists.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddHours(validityInHours);

        var invitation = new Invitation(Guid.NewGuid(), businessId, email, roleId, token, expiry);
        invitation.AddDomainEvent(new InvitationSentDomainEvent(invitation.Id));
        return invitation;
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

    private Invitation() { InvitedUserEmail = string.Empty; Token = string.Empty; Role = null!; Business = null!; } // EF Core
}