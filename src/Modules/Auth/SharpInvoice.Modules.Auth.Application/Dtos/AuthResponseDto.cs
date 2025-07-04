namespace SharpInvoice.Modules.Auth.Application.Dtos;

using System.ComponentModel;

public record AuthResponseDto(
    [property: Description("The unique identifier for the user.")] string UserId,
    [property: Description("The user's email address.")] string Email,
    [property: Description("The JSON Web Token (JWT) used for authenticating subsequent requests.")] string Token,
    [property: Description("The token used to obtain a new JWT when the old one expires.")] string RefreshToken,
    bool RequiresProfileCompletion = false
);