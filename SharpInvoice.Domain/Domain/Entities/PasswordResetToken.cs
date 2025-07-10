namespace SharpInvoice.Core.Domain.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;

public sealed class PasswordResetToken : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string Token { get; private init; }
    public string UserEmail { get; private init; }
    public DateTime ExpiryDate { get; private init; }
    public bool IsUsed { get; private set; }

    // Constructor
    public PasswordResetToken(string token, string userEmail, int validForMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);

        Id = KeyGenerator.Generate("prt", userEmail);
        Token = token;
        UserEmail = userEmail;
        ExpiryDate = DateTime.UtcNow.AddMinutes(validForMinutes);
    }

    // Methods
    public void MarkAsUsed() => IsUsed = true;
}