namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Application.Commands;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SharpInvoice.Shared.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using SharpInvoice.Shared.Infrastructure.Configuration;

public class AuthService(
    AppDbContext context,
    IBusinessService businessService,
    ITeamMemberService teamMemberService,
    IJwtTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IHttpContextAccessor httpContextAccessor,
    AppSettings appSettings) : IAuthService
{
    private readonly string _appUrl = appSettings.AppUrl;

    public async Task<RegisterResponseDto> RegisterAndCreateBusinessAsync(RegisterUserCommand command)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email))
            throw new InvalidOperationException("User with this email already exists.");

        var user = User.Create(command.Email, command.FirstName, command.LastName, BCrypt.Net.BCrypt.HashPassword(command.Password));
        await context.Users.AddAsync(user);

        await businessService.CreateBusinessForUserAsync(user.Id, command.BusinessName, user.Email, command.Country);

        var confirmationLink = $"{_appUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(user.EmailConfirmationToken!)}";
        var templateData = new Dictionary<string, string> { { "name", user.FirstName }, { "link", confirmationLink } };
        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.EmailConfirmation, templateData);
        await emailSender.SendEmailAsync(user.Email, "Confirm Your SharpInvoice Account", emailBody);

        await context.SaveChangesAsync();
        return new RegisterResponseDto(user.Id.ToString(), "Registration successful. Please check your email to confirm your account.");
    }

    public async Task<LoginResponseDto> LoginAsync(LoginUserCommand command)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.EmailConfirmed)
            throw new InvalidOperationException("Please confirm your email before logging in.");

        if (user.TwoFactorEnabled)
        {
            user.GenerateTwoFactorCode();
            await context.SaveChangesAsync();
            var templateData = new Dictionary<string, string> { { "name", user.FirstName }, { "code", user.TwoFactorCode! } };
            var emailBody = await templateRenderer.RenderAsync(EmailTemplate.TwoFactorAuth, templateData);
            await emailSender.SendEmailAsync(user.Email, "Your SharpInvoice Verification Code", emailBody);
            return new LoginResponseDto(true, "A verification code has been sent to your email.");
        }

        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        if (businessId == Guid.Empty)
            throw new InvalidOperationException("User is not associated with any active business.");

        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = Enumerable.Empty<string>();

        var authResponse = await GenerateAndSaveTokens(user, businessId, roles, permissions, false);
        return new LoginResponseDto(false, AuthResponse: authResponse);
    }

    public async Task<AuthResponseDto> HandleExternalLoginAsync()
    {
        var info = await GetExternalLoginInfoAsync() ?? throw new InvalidOperationException("Could not get external login info.");
        var externalLogin = await context.ExternalLogins.FirstOrDefaultAsync(
            el => el.LoginProvider == info.LoginProvider && el.ProviderKey == info.ProviderKey);

        User? user;
        bool isNewUser = false;
        
        if (externalLogin != null)
        {
            user = await context.Users.FindAsync(externalLogin.UserId);
            if (user == null)
            {
                // This case should not happen in a consistent database.
                throw new InvalidOperationException("User not found for existing external login.");
            }
        }
        else
        {
            user = await context.Users.FirstOrDefaultAsync(u => u.Email == info.Principal.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                // User doesn't exist, create a new one.
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? throw new InvalidOperationException("Email claim not found.");
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

                user = User.Create(email, firstName, lastName, ""); // No password for external logins
                user.ConfirmEmail(); // Email is considered confirmed from external provider
                context.Users.Add(user);
                isNewUser = true;
            }

            var newExternalLogin = ExternalLogin.Create(user.Id, info.LoginProvider, info.ProviderKey);
            context.ExternalLogins.Add(newExternalLogin);
        }

        if (user is null)
        {
            // This should be an impossible state if the logic above is correct.
            throw new InvalidOperationException("Could not determine user after external login flow.");
        }

        // Ensure business exists, create if not
        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        var requiresProfileCompletion = businessId == Guid.Empty || isNewUser;
        
        // Generate tokens even if profile completion is needed
        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId == Guid.Empty ? Guid.Empty : businessId);
        var permissions = Enumerable.Empty<string>();
        
        // Save the user and external login
        await context.SaveChangesAsync();
        
        // Return the auth response with the profile completion flag
        return await GenerateAndSaveTokens(
            user, 
            businessId == Guid.Empty ? Guid.Empty : businessId,
            roles, 
            permissions, 
            requiresProfileCompletion
        );
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordCommand command)
    {
        var resetToken = await context.PasswordResetTokens.SingleOrDefaultAsync(
            t => t.Token == command.Token && t.UserEmail == command.Email && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow);

        if (resetToken == null) return false;

        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.Email);
        if (user == null) return false;

        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(command.NewPassword));
        resetToken.MarkAsUsed();

        await context.SaveChangesAsync();
        return true;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordCommand command)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.Email);
        if (user == null) return;
        
        // Use secure random number generation
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        var resetToken = PasswordResetToken.Create(token, user.Email, 15);
        await context.PasswordResetTokens.AddAsync(resetToken);
        await context.SaveChangesAsync();

        var resetLink = $"{_appUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var templateData = new Dictionary<string, string>
            {
                { "name", user.FirstName },
                { "link", resetLink }
            };
        var emailBody = await templateRenderer.RenderAsync(EmailTemplate.PasswordReset, templateData);

        await emailSender.SendEmailAsync(user.Email, "Reset Your Password", emailBody);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken)
            ?? throw new InvalidTokenException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new InvalidTokenException("Expired refresh token.");

        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        if (businessId == Guid.Empty)
            throw new InvalidOperationException("User is not associated with any active business.");

        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = Enumerable.Empty<string>();

        return await GenerateAndSaveTokens(user, businessId, roles, permissions, false);
    }

    public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
    {
        return new AuthenticationProperties { RedirectUri = redirectUrl, Items = { { "LoginProvider", provider } } };
    }

    public async Task<AuthResponseDto> VerifyTwoFactorCodeAsync(VerifyTwoFactorCommand command)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.TwoFactorEnabled || user.TwoFactorCode != command.Code || user.TwoFactorCodeExpiry < DateTime.UtcNow)
            throw new InvalidTokenException("Invalid or expired 2FA code.");

        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        if (businessId == Guid.Empty)
            throw new InvalidOperationException("User is not associated with any active business.");
        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = Enumerable.Empty<string>();

        user.ClearTwoFactorCode();
        return await GenerateAndSaveTokens(user, businessId, roles, permissions, false);
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null || user.EmailConfirmationToken != token || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.ConfirmEmail();
        await context.SaveChangesAsync();
        return true;
    }

    public async Task EnableTwoFactorAuthenticationAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId) ?? throw new NotFoundException("User not found.");
        user.EnableTwoFactor();
        await context.SaveChangesAsync();
    }

    public async Task DisableTwoFactorAuthenticationAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId) ?? throw new NotFoundException("User not found.");
        user.DisableTwoFactor();
        await context.SaveChangesAsync();
    }

    private async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            var result = await httpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            if (result?.Succeeded != true) return null;
            principal = result.Principal;
        }

        if (principal == null)
        {
            return null;
        }

        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = principal.FindFirstValue(ClaimTypes.AuthenticationMethod);

        if (providerKey == null || provider == null)
        {
            return null;
        }

        return new ExternalLoginInfo(principal, provider, providerKey, provider);
    }

    private async Task<AuthResponseDto> GenerateAndSaveTokens(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion)
    {
        var token = tokenGenerator.GenerateToken(user, businessId, roles, permissions);
        var refreshToken = user.GenerateRefreshToken();
        await context.SaveChangesAsync();
        return new AuthResponseDto(user.Id.ToString(), user.Email, token, refreshToken, requiresProfileCompletion);
    }
}