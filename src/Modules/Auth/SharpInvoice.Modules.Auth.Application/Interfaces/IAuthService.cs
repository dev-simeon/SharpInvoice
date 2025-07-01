namespace SharpInvoice.Modules.Auth.Application.Interfaces;

using Application.Commands;
using Application.Dtos;
using Microsoft.AspNetCore.Authentication;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAndCreateBusinessAsync(RegisterUserCommand command);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
    Task<AuthResponseDto> HandleExternalLoginAsync();
    Task ForgotPasswordAsync(ForgotPasswordCommand command);
    Task<bool> ResetPasswordAsync(ResetPasswordCommand command);
    Task<bool> ConfirmEmailAsync(Guid userId, string token);

    // 2FA Methods
    Task<LoginResponseDto> LoginAsync(LoginUserCommand command);
    Task<AuthResponseDto> VerifyTwoFactorCodeAsync(VerifyTwoFactorCommand command);
    Task EnableTwoFactorAuthenticationAsync(Guid userId);
    Task DisableTwoFactorAuthenticationAsync(Guid userId);
}