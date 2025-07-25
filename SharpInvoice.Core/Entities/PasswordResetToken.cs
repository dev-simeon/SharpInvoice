namespace SharpInvoice.Core.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;
using SharpInvoice.Core.Shared;

public sealed class PasswordResetToken : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string Token { get; private init; }
    public string UserId { get; private init; }
    public string UserEmail { get; private init; }
    public DateTime ExpiryDate { get; private init; }
    public bool IsUsed { get; private set; }

    // EF Core constructor
    private PasswordResetToken() { }

    // Constructor
    public PasswordResetToken(string token, string userId, string userEmail, int validForMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);

        Id = KeyGenerator.Generate("prt", userEmail);
        Token = token;
        UserId = userId;
        UserEmail = userEmail;
        ExpiryDate = DateTime.UtcNow.AddMinutes(validForMinutes);
    }

    // Methods
    public void MarkAsUsed() => IsUsed = true;
}