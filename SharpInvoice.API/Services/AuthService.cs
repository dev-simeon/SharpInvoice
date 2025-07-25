using FluentValidation;
using Microsoft.Extensions.Logging;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiValidationException = SharpInvoice.API.Middleware.Exceptions.ValidationException;
using SharpInvoice.API.Middleware.Exceptions;
using SharpInvoice.Core.Entities;

namespace SharpInvoice.API.Services;

/// <summary>
/// Provides authentication services with improved validation, logging, and performance.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ICachingService _cachingService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="cachingService">The caching service.</param>
    /// <param name="registerValidator">The registration validator.</param>
    /// <param name="loginValidator">The login validator.</param>
    /// <param name="logger">The logger.</param>
    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        ICachingService cachingService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));
        _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
        _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

        // Validate request using FluentValidation
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Registration validation failed for email {Email}: {Errors}", request.Email, errors);
            throw new ApiValidationException(errors);
        }

        // Check if email exists (with caching for better performance)
        var cacheKey = $"email_exists_{request.Email.ToLowerInvariant()}";
        var emailExists = await _cachingService.GetAsync<bool?>(cacheKey);
        
        if (emailExists is null)
        {
            emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
            await _cachingService.SetAsync(cacheKey, emailExists, TimeSpan.FromMinutes(5));
        }

        if (emailExists == true)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw new ApiValidationException("Email is already registered.");
        }

        try
        {
            // Create user with transaction
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var user = User.Register(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    _passwordHasher.HashPassword(request.Password),
                    request.PhoneNumber
                );

                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate the email exists cache
                await _cachingService.RemoveAsync(cacheKey);

                // Send email confirmation asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendEmailConfirmationAsync(user.Email, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email confirmation for user: {Email}", user.Email);
                    }
                }, cancellationToken);

                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return await GenerateTokensForUserAsync(user, cancellationToken);
            });
        }
        catch (Exception ex) when (ex is not ApiValidationException)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails.</exception>
    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Starting user authentication for email: {Email}", request.Email);

        // Validate request using FluentValidation
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Login validation failed for email {Email}: {Errors}", request.Email, errors);
            throw new ApiValidationException(errors);
        }

        try
        {
            // Find user with caching
            var cacheKey = $"user_by_email_{request.Email.ToLowerInvariant()}";
            var user = await _cachingService.GetAsync<User>(cacheKey);
            
            if (user is null)
            {
                user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (user is not null)
                {
                    await _cachingService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(15));
                }
            }

            if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed for email: {Email}", request.Email);
                throw new UnauthorizedException("Invalid email or password.");
            }

            _logger.LogInformation("User authenticated successfully: {Email}", request.Email);
            return await GenerateTokensForUserAsync(user, cancellationToken);
        }
        catch (Exception ex) when (ex is not ApiValidationException and not UnauthorizedException)
        {
            _logger.LogError(ex, "Error during user authentication for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Refreshes authentication tokens.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the refresh token is invalid.</exception>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Refresh token request with empty token");
            throw new ApiValidationException("Refresh token is required.");
        }

        _logger.LogInformation("Starting token refresh");

        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshToken);
            if (principal?.Identity?.Name is null)
            {
                _logger.LogWarning("Invalid token principal in refresh request");
                throw new UnauthorizedException("Invalid token.");
            }

            var userId = principal.Identity.Name;
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user is null)
            {
                _logger.LogWarning("User not found during token refresh: {UserId}", userId);
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token for user: {UserId}", userId);
                throw new UnauthorizedException("Invalid refresh token.");
            }

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);
            return await GenerateTokensForUserAsync(user, cancellationToken);
        }
        catch (Exception ex) when (ex is not ApiValidationException and not UnauthorizedException)
        {
            _logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }

    /// <summary>
    /// Sends an email confirmation message.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Email confirmation requested with empty email");
            return;
        }

        _logger.LogInformation("Sending email confirmation to: {Email}", email);

        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user is null || user.EmailConfirmed)
            {
                _logger.LogInformation("Email confirmation not needed for: {Email}", email);
                return;
            }

            user.GenerateEmailConfirmationToken();
            await _unitOfWork.SaveChangesAsync();

            var confirmationLink = $"https://your-frontend-url/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.EmailConfirmationToken!)}";
            
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your email address",
                $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.",
                cancellationToken);

            _logger.LogInformation("Email confirmation sent successfully to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email confirmation to: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Confirms a user's email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="token">The confirmation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if confirmation is successful; otherwise, false.</returns>
    public async Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Email confirmation attempted with missing email or token");
            return false;
        }

        _logger.LogInformation("Confirming email for: {Email}", email);

        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user is null || 
                user.EmailConfirmationToken != token || 
                user.EmailConfirmationTokenExpiry is null || 
                user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid email confirmation attempt for: {Email}", email);
                return false;
            }

            user.ConfirmEmail();
            await _unitOfWork.SaveChangesAsync();

            // Invalidate user cache
            var cacheKey = $"user_by_email_{email.ToLowerInvariant()}";
            await _cachingService.RemoveAsync(cacheKey);

            _logger.LogInformation("Email confirmed successfully for: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Requests a password reset.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Password reset requested with empty email");
            return;
        }

        _logger.LogInformation("Password reset requested for: {Email}", email);

        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user is null)
            {
                // Don't reveal whether the user exists or not
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return;
            }

            // Generate password reset token
            var resetToken = await _unitOfWork.PasswordResetTokens.CreateForUserAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();

            var resetLink = $"https://your-frontend-url/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken.Token)}";
            
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"You can reset your password by <a href='{resetLink}'>clicking here</a>. This link will expire in 1 hour.",
                cancellationToken);

            _logger.LogInformation("Password reset email sent to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if reset is successful; otherwise, false.</returns>
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Password reset attempted for: {Email}", request.Email);

        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                return false;
            }

            var resetToken = await _unitOfWork.PasswordResetTokens.GetValidTokenAsync(user.Id, request.Token);
            if (resetToken is null)
            {
                _logger.LogWarning("Invalid password reset token for: {Email}", request.Email);
                return false;
            }

            // Reset password with transaction
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                user.UpdatePassword(_passwordHasher.HashPassword(request.NewPassword));
                resetToken.MarkAsUsed();
                
                await _unitOfWork.SaveChangesAsync();

                // Invalidate user cache
                var cacheKey = $"user_by_email_{request.Email.ToLowerInvariant()}";
                await _cachingService.RemoveAsync(cacheKey);

                _logger.LogInformation("Password reset successfully for: {Email}", request.Email);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for: {Email}", request.Email);
            return false;
        }
    }

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user information.</returns>
    public async Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        _logger.LogInformation("Getting current user information for: {UserId}", userId);

        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                throw new NotFoundException("User", userId);
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Role = user.ApplicationUserRole.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error getting user information for: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Generates access and refresh tokens for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    private async Task<TokenResponse> GenerateTokensForUserAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        _logger.LogDebug("Generating tokens for user: {UserId}", user.Id);

        try
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("role", user.ApplicationUserRole.ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Tokens generated successfully for user: {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tokens for user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Enables or disables two-factor authentication for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="enable">Whether to enable or disable 2FA.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The 2FA status response.</returns>
    public async Task<TwoFactorStatusResponse> SetTwoFactorAsync(string userId, bool enable, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting two-factor authentication to {Enable} for user: {UserId}", enable, userId);

            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(userId), cancellationToken)
                ?? throw new NotFoundException($"User with ID {userId} not found");

            if (enable)
            {
                user.EnableTwoFactor();
                _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);
            }
            else
            {
                user.DisableTwoFactor();
                _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);
            }

            await _unitOfWork.SaveChangesAsync();

            return new TwoFactorStatusResponse
            {
                IsEnabled = user.TwoFactorEnabled,
                Message = enable ? "Two-factor authentication has been enabled." : "Two-factor authentication has been disabled."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting two-factor authentication for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Generates a two-factor authentication code for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateTwoFactorCodeAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating two-factor code for user: {UserId}", userId);

            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(userId), cancellationToken)
                ?? throw new NotFoundException($"User with ID {userId} not found");

            if (!user.TwoFactorEnabled)
            {
                throw new ApiValidationException("Two-factor authentication is not enabled for this user");
            }

            user.GenerateTwoFactorCode();
            await _unitOfWork.SaveChangesAsync();

            // Send 2FA code via email
            if (!string.IsNullOrEmpty(user.TwoFactorCode))
            {
                await _emailService.SendTwoFactorCodeAsync(user.Email, user.TwoFactorCode, cancellationToken);
                _logger.LogInformation("Two-factor code sent to user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating two-factor code for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Verifies a two-factor authentication code.
    /// </summary>
    /// <param name="request">The verification request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if verification is successful; otherwise, false.</returns>
    public async Task<bool> VerifyTwoFactorCodeAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying two-factor code for email: {Email}", request.Email);

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email)
                ?? throw new NotFoundException($"User with email {request.Email} not found");

            if (!user.TwoFactorEnabled)
            {
                _logger.LogWarning("Two-factor authentication not enabled for user: {Email}", request.Email);
                return false;
            }

            if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCodeExpiry == null)
            {
                _logger.LogWarning("No active two-factor code for user: {Email}", request.Email);
                return false;
            }

            if (user.TwoFactorCodeExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Two-factor code expired for user: {Email}", request.Email);
                user.ClearTwoFactorCode();
                await _unitOfWork.SaveChangesAsync();
                return false;
            }

            if (user.TwoFactorCode != request.Code)
            {
                _logger.LogWarning("Invalid two-factor code provided for user: {Email}", request.Email);
                return false;
            }

            // Clear the code after successful verification
            user.ClearTwoFactorCode();
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Two-factor code verified successfully for user: {Email}", request.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying two-factor code for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Gets the two-factor authentication status for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The 2FA status response.</returns>
    public async Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting two-factor status for user: {UserId}", userId);

            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(userId), cancellationToken)
                ?? throw new NotFoundException($"User with ID {userId} not found");

            return new TwoFactorStatusResponse
            {
                IsEnabled = user.TwoFactorEnabled,
                Message = user.TwoFactorEnabled ? "Two-factor authentication is enabled." : "Two-factor authentication is disabled."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting two-factor status for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Initiates OAuth authentication with external provider.
    /// </summary>
    /// <param name="provider">The OAuth provider (google, facebook).</param>
    /// <param name="returnUrl">Optional return URL after authentication.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>OAuth URL response with authentication URL and state.</returns>
    public async Task<OAuthUrlResponse> InitiateOAuthAsync(string provider, string? returnUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initiating OAuth authentication with provider: {Provider}", provider);

            // Validate provider
            if (!IsValidOAuthProvider(provider))
            {
                throw new ApiValidationException($"Unsupported OAuth provider: {provider}");
            }

            // Generate state for CSRF protection
            var state = Guid.NewGuid().ToString("N");

            // Store state in cache for verification
            var cacheKey = $"oauth_state_{state}";
            await _cachingService.SetAsync(cacheKey, new { provider, returnUrl, createdAt = DateTime.UtcNow }, TimeSpan.FromMinutes(10), cancellationToken);

            // Build OAuth URL (this would typically use the actual OAuth provider configuration)
            var authUrl = provider.ToLowerInvariant() switch
            {
                "google" => $"/api/v1/auth/oauth/google?state={state}&return_url={Uri.EscapeDataString(returnUrl ?? "")}",
                "facebook" => $"/api/v1/auth/oauth/facebook?state={state}&return_url={Uri.EscapeDataString(returnUrl ?? "")}",
                _ => throw new ApiValidationException($"Unsupported OAuth provider: {provider}")
            };

            _logger.LogInformation("OAuth URL generated for provider: {Provider}, state: {State}", provider, state);

            return new OAuthUrlResponse
            {
                AuthUrl = authUrl,
                State = state
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating OAuth authentication with provider: {Provider}", provider);
            throw;
        }
    }

    /// <summary>
    /// Handles OAuth callback and authenticates user.
    /// </summary>
    /// <param name="request">The OAuth callback request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication tokens if successful.</returns>
    public async Task<TokenResponse> HandleOAuthCallbackAsync(OAuthCallbackRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling OAuth callback for provider: {Provider}, state: {State}", request.Provider, request.State);

            // Validate state parameter
            var cacheKey = $"oauth_state_{request.State}";
            var stateData = await _cachingService.GetAsync<dynamic>(cacheKey, cancellationToken);
            
            if (stateData == null)
            {
                throw new ApiValidationException("Invalid or expired OAuth state parameter");
            }

            // Remove state from cache
            await _cachingService.RemoveAsync(cacheKey, cancellationToken);

            // Check for OAuth error
            if (!string.IsNullOrEmpty(request.Error))
            {
                _logger.LogWarning("OAuth provider returned error: {Error}", request.Error);
                throw new ApiValidationException($"OAuth authentication failed: {request.Error}");
            }

            // This is a placeholder implementation
            // In a real implementation, you would:
            // 1. Exchange the authorization code for an access token
            // 2. Use the access token to get user info from the OAuth provider
            // 3. Either create a new user or find existing user by email
            // 4. Generate JWT tokens for the user

            // For now, return a placeholder response
            _logger.LogWarning("OAuth callback handling is not fully implemented. This is a placeholder.");
            
            throw new NotImplementedException("OAuth callback handling requires full implementation with actual OAuth provider integration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for provider: {Provider}", request.Provider);
            throw;
        }
    }

    /// <summary>
    /// Validates if the OAuth provider is supported.
    /// </summary>
    /// <param name="provider">The OAuth provider name.</param>
    /// <returns>True if the provider is supported; otherwise, false.</returns>
    private static bool IsValidOAuthProvider(string provider)
    {
        var supportedProviders = new[] { "google", "facebook" };
        return supportedProviders.Contains(provider.ToLowerInvariant());
    }
} 