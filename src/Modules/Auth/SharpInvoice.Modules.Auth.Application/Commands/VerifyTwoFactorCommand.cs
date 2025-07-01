namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record VerifyTwoFactorCommand(
    [property: Required, EmailAddress, Description("The user's email address, used to identify the login session.")] string Email,
    [property: Required, Length(6, 6), Description("The 6-digit verification code sent to the user's email.")] string Code
);