namespace SharpInvoice.Modules.Auth.Application.Commands;

using SharpInvoice.Modules.Auth.Application.Dtos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MediatR;

public record RegisterUserCommand(
    [property: Required, EmailAddress, Description("The user's email address.")] string Email,
    [property: Required, MinLength(2), Description("The user's first name.")] string FirstName,
    [property: Required, MinLength(2), Description("The user's last name.")] string LastName,
    [property: Required, MinLength(8), Description("The user's password. Must be at least 8 characters.")] string Password
) : IRequest<RegisterResponseDto>;