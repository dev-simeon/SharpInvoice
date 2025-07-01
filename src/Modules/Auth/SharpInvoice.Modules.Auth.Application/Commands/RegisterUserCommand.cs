namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record RegisterUserCommand(
    [property: Required, EmailAddress, Description("The user's email address.")] string Email,
    [property: Required, MinLength(2), Description("The user's first name.")] string FirstName,
    [property: Required, MinLength(2), Description("The user's last name.")] string LastName,
    [property: Required, MinLength(8), Description("The user's password. Must be at least 8 characters.")] string Password,
    [property: Required, Description("The name of the business to create.")] string BusinessName,
    [property: Required, Description("The country of the business.")] string Country
);