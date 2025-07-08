namespace SharpInvoice.Modules.Auth.Application.Interfaces;

using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Comprehensive authentication service interface that handles all authentication operations
/// </summary>
public interface IAuthService
{
    // Registration and Email Confirmation
    Task<RegisterResponseDto> RegisterUserAsync(string email, string firstName, string lastName, string password);
    Task<bool> ConfirmEmailAsync(Guid userId, string token);
    
    // Login and Authentication
    Task<LoginResponseDto> LoginAsync(string email, string password);
    Task<AuthResponseDto> VerifyTwoFactorAsync(string email, string verificationCode);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    
    // Password Management
    Task SendPasswordResetEmailAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    
    // External Authentication
    Task<AuthResponseDto> HandleExternalLoginAsync();
    Task<bool> LinkAccountAsync(Guid userId, string provider, string providerKey);
    Task UnlinkAccountAsync(Guid userId, string provider);
    Task<IEnumerable<ExternalAccountDto>> GetLinkedAccountsAsync(Guid userId);
    
    // Token Generation (existing method kept for backward compatibility)
    Task<AuthResponseDto> GenerateTokensAsync(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion);
}