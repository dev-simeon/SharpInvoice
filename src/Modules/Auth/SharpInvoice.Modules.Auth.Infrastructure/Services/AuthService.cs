namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using AuthUserRepository = SharpInvoice.Modules.Auth.Application.Interfaces.IUserRepository;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class AuthService(
    AuthUserRepository userRepository,
    IPasswordService passwordService,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IExternalLoginRepository externalLoginRepository,
    IJwtTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    IBusinessService businessService,
    ITeamMemberService teamMemberService) : IAuthService
{
    // Registration and Email Confirmation
    public async Task<RegisterResponseDto> RegisterUserAsync(string email, string firstName, string lastName, string password)
    {
        if (await userRepository.ExistsByEmailAsync(email))
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var hashedPassword = passwordService.HashPassword(password);
        var user = User.Create(email, firstName, lastName, hashedPassword);

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return new RegisterResponseDto(user.Id.ToString(), "Registration successful. Please check your email to confirm your account.");
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var isValid = user.ConfirmEmail(token);
        if (isValid)
        {
            await unitOfWork.SaveChangesAsync();
        }

        return isValid;
    }

    // Login and Authentication
    public async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null || !passwordService.VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsEmailConfirmed)
        {
            throw new InvalidOperationException("Email not confirmed. Please check your email for confirmation link.");
        }

        // Check if 2FA is enabled
        if (user.IsTwoFactorEnabled)
        {
            // Send 2FA code logic would go here
            return new LoginResponseDto { IsTwoFactorRequired = true, Message = "Two-factor authentication code has been sent." };
        }

        // Generate tokens
        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = await teamMemberService.GetUserPermissionsAsync(user.Id, businessId);

        var authResponse = await GenerateTokensAsync(user, businessId, roles, permissions, !user.IsProfileCompleted);

        return new LoginResponseDto { AuthResponse = authResponse };
    }

    public async Task<AuthResponseDto> VerifyTwoFactorAsync(string email, string verificationCode)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid user.");
        }

        // Verify 2FA code logic would go here
        var isValidCode = user.VerifyTwoFactorCode(verificationCode);
        if (!isValidCode)
        {
            throw new UnauthorizedAccessException("Invalid verification code.");
        }

        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = await teamMemberService.GetUserPermissionsAsync(user.Id, businessId);

        return await GenerateTokensAsync(user, businessId, roles, permissions, !user.IsProfileCompleted);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null || !user.IsRefreshTokenValid(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
        var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
        var permissions = await teamMemberService.GetUserPermissionsAsync(user.Id, businessId);

        return await GenerateTokensAsync(user, businessId, roles, permissions, !user.IsProfileCompleted);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null) return false;

        user.RevokeRefreshToken(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    // Password Management
    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null) return; // Don't reveal if user exists

        var token = await passwordResetTokenRepository.CreateTokenAsync(user.Id);
        
        // Send email logic
        var resetLink = $"https://yourapp.com/reset-password?token={token}&email={email}";
        await emailSender.SendEmailAsync(email, "Password Reset", $"Click here to reset your password: {resetLink}");
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var isValidToken = await passwordResetTokenRepository.ValidateTokenAsync(user.Id, token);
        if (!isValidToken) return false;

        var hashedPassword = passwordService.HashPassword(newPassword);
        user.UpdatePassword(hashedPassword);

        await passwordResetTokenRepository.UseTokenAsync(user.Id, token);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    // External Authentication
    public async Task<AuthResponseDto> HandleExternalLoginAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var info = await httpContext.AuthenticateAsync("Identity.External");
        
        if (info?.Principal == null)
        {
            throw new InvalidOperationException("External authentication failed.");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = info.Properties?.Items["LoginProvider"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey) || string.IsNullOrEmpty(provider))
        {
            throw new InvalidOperationException("Required information not provided by external provider.");
        }

        // Check if external login already exists
        var existingExternalLogin = await externalLoginRepository.GetByProviderAsync(provider, providerKey);
        if (existingExternalLogin != null)
        {
            var user = await userRepository.GetByIdAsync(existingExternalLogin.UserId);
            var businessId = await businessService.GetBusinessIdByOwnerAsync(user.Id);
            var roles = await teamMemberService.GetUserRolesAsync(user.Id, businessId);
            var permissions = await teamMemberService.GetUserPermissionsAsync(user.Id, businessId);

            return await GenerateTokensAsync(user, businessId, roles, permissions, !user.IsProfileCompleted);
        }

        // Check if user exists by email
        var existingUser = await userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            // Link the external account to existing user
            await externalLoginRepository.AddAsync(existingUser.Id, provider, providerKey);
            await unitOfWork.SaveChangesAsync();

            var businessId = await businessService.GetBusinessIdByOwnerAsync(existingUser.Id);
            var roles = await teamMemberService.GetUserRolesAsync(existingUser.Id, businessId);
            var permissions = await teamMemberService.GetUserPermissionsAsync(existingUser.Id, businessId);

            return await GenerateTokensAsync(existingUser, businessId, roles, permissions, !existingUser.IsProfileCompleted);
        }

        // Create new user
        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "User";
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";
        
        var newUser = User.CreateFromExternal(email, firstName, lastName);
        await userRepository.AddAsync(newUser);
        await externalLoginRepository.AddAsync(newUser.Id, provider, providerKey);
        await unitOfWork.SaveChangesAsync();

        // For new users, they won't have a business yet
        return await GenerateTokensAsync(newUser, Guid.Empty, Enumerable.Empty<string>(), Enumerable.Empty<string>(), true);
    }

    public async Task<bool> LinkAccountAsync(Guid userId, string provider, string providerKey)
    {
        var existingLink = await externalLoginRepository.GetByProviderAsync(provider, providerKey);
        if (existingLink != null) return false; // Already linked to another user

        await externalLoginRepository.AddAsync(userId, provider, providerKey);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task UnlinkAccountAsync(Guid userId, string provider)
    {
        await externalLoginRepository.RemoveAsync(userId, provider);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExternalAccountDto>> GetLinkedAccountsAsync(Guid userId)
    {
        var externalLogins = await externalLoginRepository.GetByUserIdAsync(userId);
        return externalLogins.Select(x => new ExternalAccountDto(x.Provider, x.ProviderDisplayName));
    }

    // Token Generation (existing method enhanced)
    public async Task<AuthResponseDto> GenerateTokensAsync(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion)
    {
        var token = tokenGenerator.GenerateToken(user, businessId, roles, permissions);
        var refreshToken = user.GenerateRefreshToken();
        await unitOfWork.SaveChangesAsync();
        return new AuthResponseDto(user.Id.ToString(), user.Email, token, refreshToken, requiresProfileCompletion);
    }
}