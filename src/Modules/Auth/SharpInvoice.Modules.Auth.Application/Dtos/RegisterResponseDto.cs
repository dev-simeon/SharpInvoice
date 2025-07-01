namespace SharpInvoice.Modules.Auth.Application.Dtos;

using System.ComponentModel;

public record RegisterResponseDto(
    [property: Description("The unique identifier for the newly created user.")] string UserId,
    [property: Description("A message indicating that a confirmation email has been sent.")] string Message
);