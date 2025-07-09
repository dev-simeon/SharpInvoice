namespace SharpInvoice.Core.Domain.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;

public sealed class PasswordResetToken : BaseEntity
{
    private PasswordResetToken(string token, string userEmail, DateTime expiryDate) 
    {
        Token = token;
        UserEmail = userEmail;
        ExpiryDate = expiryDate;
        IsUsed = false;
    }

    public static PasswordResetToken Create(string token, string userEmail, int validForMinutes)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (string.IsNullOrWhiteSpace(userEmail))
            throw new ArgumentException("User email cannot be empty.", nameof(userEmail));

        ArgumentOutOfRangeException.ThrowIfLessThan(validForMinutes, 5, "Validity period must be at least 5 minute.");

        return new PasswordResetToken(token, userEmail, DateTime.UtcNow.AddMinutes(validForMinutes));
    }

    public void MarkAsUsed() => IsUsed = true;

    public Guid Id { get; private init; }
    public string Token { get; private init; }
    public string UserEmail { get; private init; }
    public DateTime ExpiryDate { get; private init; }
    public bool IsUsed { get; private set; }

    private PasswordResetToken() { Token = string.Empty; UserEmail = string.Empty; }
}