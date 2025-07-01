namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record LoginUserCommand(
    [property: Required, EmailAddress, Description("The user's email address.")] string Email,
    [property: Required, Description("The user's password.")] string Password
);