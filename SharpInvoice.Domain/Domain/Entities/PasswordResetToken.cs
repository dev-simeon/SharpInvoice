namespace SharpInvoice.Modules.Auth.Domain.Entities;

using SharpInvoice.Core.Domain.Shared;
using System.ComponentModel.DataAnnotations;

public sealed class PasswordResetToken : Entity<Guid>
{
    [Required] public string Token { get; private init; }
    [Required][EmailAddress] public string UserEmail { get; private init; }
    [Required] public DateTime ExpiryDate { get; private init; }
    public bool IsUsed { get; private set; }

    private PasswordResetToken(Guid id, string token, string userEmail, DateTime expiryDate) : base(id)
    {
        Token = token; UserEmail = userEmail; ExpiryDate = expiryDate; IsUsed = false;
    }
    public static PasswordResetToken Create(string token, string userEmail, int validForMinutes) => new(Guid.NewGuid(), token, userEmail, DateTime.UtcNow.AddMinutes(validForMinutes));
    public void MarkAsUsed() => IsUsed = true;
    private PasswordResetToken() { Token = string.Empty; UserEmail = string.Empty; } // EF Core
}