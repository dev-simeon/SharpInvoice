namespace SharpInvoice.Modules.Auth.Application.Dtos;

using System.ComponentModel;
using System.Text.Json.Serialization;

public record LoginResponseDto(
    [property: Description("Indicates if a two-factor authentication step is required.")] bool IsTwoFactorRequired,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("A message for the user, typically explaining the 2FA requirement.")] string? Message = "",
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("The authentication tokens, provided only if 2FA is not required.")] AuthResponseDto? AuthResponse = null
);