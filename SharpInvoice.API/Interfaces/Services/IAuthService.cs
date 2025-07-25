using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

/// <summary>
/// Interface for authentication services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes authentication tokens.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email confirmation message.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a user's email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="token">The confirmation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if confirmation is successful; otherwise, false.</returns>
    Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a password reset.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if reset is successful; otherwise, false.</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user information.</returns>
    Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables two-factor authentication for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="enable">Whether to enable or disable 2FA.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The 2FA status response.</returns>
    Task<TwoFactorStatusResponse> SetTwoFactorAsync(string userId, bool enable, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a two-factor authentication code for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GenerateTwoFactorCodeAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a two-factor authentication code.
    /// </summary>
    /// <param name="request">The verification request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if verification is successful; otherwise, false.</returns>
    Task<bool> VerifyTwoFactorCodeAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the two-factor authentication status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The 2FA status response.</returns>
    Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates OAuth authentication with external provider.
    /// </summary>
    /// <param name="provider">The OAuth provider (google, facebook).</param>
    /// <param name="returnUrl">Optional return URL after authentication.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>OAuth URL response with authentication URL and state.</returns>
    Task<OAuthUrlResponse> InitiateOAuthAsync(string provider, string? returnUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles OAuth callback and authenticates user.
    /// </summary>
    /// <param name="request">The OAuth callback request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens if successful.</returns>
    Task<TokenResponse> HandleOAuthCallbackAsync(OAuthCallbackRequest request, CancellationToken cancellationToken = default);
} 