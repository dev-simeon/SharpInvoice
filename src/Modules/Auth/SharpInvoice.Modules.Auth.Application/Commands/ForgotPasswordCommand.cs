namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record ForgotPasswordCommand(
    [property: Required, EmailAddress, Description("The email address of the user who forgot their password.")] string Email
);