namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record ResetPasswordCommand(
    [property: Required, EmailAddress, Description("The user's email address.")] string Email,
    [property: Required, Description("The password reset token sent to the user's email.")] string Token,
    [property: Required, MinLength(8), Description("The new password. Must be at least 8 characters.")] string NewPassword
);