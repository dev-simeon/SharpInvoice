namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record RefreshTokenCommand(
    [property: Required, Description("The refresh token used to obtain a new access token.")] string RefreshToken
) : IRequest<AuthResponseDto>;