namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;

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
        BusinessId = businessId; InvitedUserEmail = email; RoleId = roleId; Token = token; ExpiryDate = expiry; Status = InvitationStatus.Pending;
    }
    public static Invitation Create(Guid businessId, string email, Guid roleId, string token, int hours) => new(Guid.NewGuid(), businessId, email, roleId, token, DateTime.UtcNow.AddHours(hours));
    public void Accept() { if (Status == InvitationStatus.Pending) Status = InvitationStatus.Accepted; }
    public void Expire() { if (Status == InvitationStatus.Pending) Status = InvitationStatus.Expired; }
    private Invitation() { InvitedUserEmail = string.Empty; Token = string.Empty; Role = null!; Business = null!; } // EF Core
}