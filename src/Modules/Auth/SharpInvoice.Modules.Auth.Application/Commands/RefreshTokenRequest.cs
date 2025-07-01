namespace SharpInvoice.Modules.Auth.Application.Commands;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record RefreshTokenRequest(
    [property: Required, Description("The refresh token used to obtain a new access token.")] string RefreshToken
); 