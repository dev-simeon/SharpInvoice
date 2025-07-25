using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="logger">The logger.</param>
    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>

    /// <remarks>
    /// Creates a new user account with the provided credentials. The password must meet security requirements:
    /// - At least 8 characters long
    /// - Contains uppercase and lowercase letters
    /// - Contains at least one digit
    /// - Contains at least one special character (@$!%*?&amp;)
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/register
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePass123!",
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "phoneNumber": "+1234567890"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The user registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT access and refresh tokens</returns>
    /// <response code="201">User registered successfully. Returns JWT tokens.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="409">Email address is already registered.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid registration data provided.");
        }

        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        var result = await _authService.RegisterAsync(request, cancellationToken);

        _logger.LogInformation("User registered successfully with email: {Email}", request.Email);

        return CreatedAtAction(nameof(GetCurrentUserAsync), new { }, result);
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <remarks>
    /// Authenticates a user with email and password credentials. Returns JWT access and refresh tokens on successful authentication.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/login
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePass123!"
    ///     }
    /// 
    /// The returned access token should be included in the Authorization header for protected endpoints:
    /// `Authorization: Bearer {accessToken}`
    /// </remarks>
    /// <param name="request">The login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT access and refresh tokens</returns>
    /// <response code="200">Authentication successful. Returns JWT tokens.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <response code="429">Too many login attempts. Rate limit exceeded.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid login credentials provided.");
        }

        _logger.LogInformation("Attempting to authenticate user with email: {Email}", request.Email);

        var result = await _authService.LoginAsync(request, cancellationToken);

        _logger.LogInformation("User authenticated successfully with email: {Email}", request.Email);

        return Success(result);
    }

    /// <summary>
    /// Refreshes authentication tokens.
    /// </summary>
    /// <param name="request">The token refresh request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Invalid("Invalid refresh token provided.");
        }

        _logger.LogInformation("Attempting to refresh token");

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        _logger.LogInformation("Token refreshed successfully");

        return Success(result);
    }

    /// <summary>
    /// Logs a user out by blacklisting their token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
        
        if (authorizationHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var token = authorizationHeader["Bearer ".Length..].Trim();
            
            _logger.LogInformation("Logging out user with ID: {UserId}", CurrentUserId);
            
            await _tokenService.BlacklistTokenAsync(token);
            
            _logger.LogInformation("User logged out successfully with ID: {UserId}", CurrentUserId);
        }

        return Success();
    }

    /// <summary>
    /// Confirms a user's email address
    /// </summary>
    /// <remarks>
    /// Confirms a user's email address using the confirmation token sent via email during registration.
    /// This endpoint is typically called by clicking a link in the confirmation email.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/auth/confirm-email?email=user@example.com&amp;token=abc123xyz
    /// 
    /// </remarks>
    /// <param name="email">The user's email address to confirm</param>
    /// <param name="token">The email confirmation token received via email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation result message</returns>
    /// <response code="200">Email confirmed successfully.</response>
    /// <response code="400">Invalid email, token, or expired confirmation link.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmailAsync(
        [FromQuery(Name = "email"), Required, EmailAddress] string email,
        [FromQuery(Name = "token"), Required] string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return Invalid("Email and token are required.");
        }

        _logger.LogInformation("Attempting to confirm email for: {Email}", email);

        var result = await _authService.ConfirmEmailAsync(email, token, cancellationToken);

        if (result)
        {
            _logger.LogInformation("Email confirmed successfully for: {Email}", email);
            return Success(new { Message = "Email confirmed successfully." });
        }

        _logger.LogWarning("Email confirmation failed for: {Email}", email);
        return Invalid("Invalid email confirmation link.");
    }

    /// <summary>
    /// Requests a password reset.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success message.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid email address provided.");
        }

        _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

        await _authService.RequestPasswordResetAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        return Success(new { Message = "If the email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success message or error.</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid password reset data provided.");
        }

        _logger.LogInformation("Attempting to reset password for email: {Email}", request.Email);

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);

        if (result)
        {
            _logger.LogInformation("Password reset successfully for email: {Email}", request.Email);
            return Success(new { Message = "Password reset successfully." });
        }

        _logger.LogWarning("Password reset failed for email: {Email}", request.Email);
        return Invalid("Invalid password reset link.");
    }

    /// <summary>
    /// Gets the current authenticated user's profile information
    /// </summary>
    /// <remarks>
    /// Retrieves the profile information for the currently authenticated user based on the JWT token.
    /// Requires valid Bearer token in the Authorization header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/auth/me
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user's profile information</returns>
    /// <response code="200">User profile retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await _authService.GetCurrentUserAsync(CurrentUserId!, cancellationToken);
        return Success(currentUser);
    }

    /// <summary>
    /// Enables or disables two-factor authentication for the current user
    /// </summary>
    /// <remarks>
    /// Enables or disables two-factor authentication for the authenticated user. When enabling 2FA,
    /// users will need to provide a verification code sent via email for future logins.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/two-factor
    ///     Authorization: Bearer {your-access-token}
    ///     {
    ///         "enable": true
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The 2FA enable/disable request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated 2FA status</returns>
    /// <response code="200">Two-factor authentication status updated successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("two-factor")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetTwoFactorAsync(
        [FromBody] EnableTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid two-factor authentication request.");
        }

        _logger.LogInformation("Setting two-factor authentication to {Enable} for user: {UserId}", request.Enable, CurrentUserId);

        var result = await _authService.SetTwoFactorAsync(CurrentUserId!, request.Enable, cancellationToken);

        _logger.LogInformation("Two-factor authentication updated for user: {UserId}", CurrentUserId);

        return Success(result);
    }

    /// <summary>
    /// Gets the current user's two-factor authentication status
    /// </summary>
    /// <remarks>
    /// Retrieves the current two-factor authentication status for the authenticated user.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/auth/two-factor/status
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current 2FA status</returns>
    /// <response code="200">Two-factor authentication status retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("two-factor/status")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting two-factor status for user: {UserId}", CurrentUserId);

        var result = await _authService.GetTwoFactorStatusAsync(CurrentUserId!, cancellationToken);

        return Success(result);
    }

    /// <summary>
    /// Generates a two-factor authentication code for the current user
    /// </summary>
    /// <remarks>
    /// Generates and sends a 6-digit verification code via email for two-factor authentication.
    /// This endpoint is typically used during login when 2FA is enabled.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/two-factor/generate
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message indicating code was sent</returns>
    /// <response code="200">Two-factor code generated and sent successfully.</response>
    /// <response code="400">Two-factor authentication is not enabled for this user.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("two-factor/generate")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateTwoFactorCodeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating two-factor code for user: {UserId}", CurrentUserId);

        await _authService.GenerateTwoFactorCodeAsync(CurrentUserId!, cancellationToken);

        _logger.LogInformation("Two-factor code generated for user: {UserId}", CurrentUserId);

        return Success(new { Message = "Two-factor authentication code has been sent to your email." });
    }

    /// <summary>
    /// Verifies a two-factor authentication code
    /// </summary>
    /// <remarks>
    /// Verifies a 6-digit two-factor authentication code provided by the user.
    /// This endpoint is used during login when 2FA is enabled.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/two-factor/verify
    ///     {
    ///         "email": "user@example.com",
    ///         "code": "123456"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The 2FA verification request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    /// <response code="200">Two-factor code verified successfully.</response>
    /// <response code="400">Invalid input data, validation failed, or verification failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("two-factor/verify")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyTwoFactorCodeAsync(
        [FromBody] VerifyTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid two-factor verification request.");
        }

        _logger.LogInformation("Verifying two-factor code for email: {Email}", request.Email);

        var isValid = await _authService.VerifyTwoFactorCodeAsync(request, cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("Two-factor code verification failed for email: {Email}", request.Email);
            return Invalid("Invalid or expired two-factor authentication code.");
        }

        _logger.LogInformation("Two-factor code verified successfully for email: {Email}", request.Email);

        return Success(new { Message = "Two-factor authentication code verified successfully." });
    }

    /// <summary>
    /// Initiates OAuth authentication with external provider
    /// </summary>
    /// <remarks>
    /// Initiates OAuth authentication flow with supported external providers (Google, Facebook).
    /// Returns an authentication URL that the client should redirect to.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/oauth/initiate
    ///     {
    ///         "provider": "google",
    ///         "returnUrl": "https://myapp.com/auth/callback"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The OAuth initiation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OAuth authentication URL and state</returns>
    /// <response code="200">OAuth authentication URL generated successfully.</response>
    /// <response code="400">Invalid provider or validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("oauth/initiate")]
    [ProducesResponseType(typeof(OAuthUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitiateOAuthAsync(
        [FromBody] OAuthLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid OAuth request.");
        }

        _logger.LogInformation("Initiating OAuth authentication with provider: {Provider}", request.Provider);

        var result = await _authService.InitiateOAuthAsync(request.Provider, request.ReturnUrl, cancellationToken);

        _logger.LogInformation("OAuth URL generated for provider: {Provider}", request.Provider);

        return Success(result);
    }

    /// <summary>
    /// Handles OAuth callback from external provider
    /// </summary>
    /// <remarks>
    /// Handles the OAuth callback from external authentication providers.
    /// This endpoint is called by the OAuth provider after user authorization.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/oauth/callback
    ///     {
    ///         "provider": "google",
    ///         "code": "auth_code_from_provider",
    ///         "state": "csrf_protection_state"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The OAuth callback request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT access and refresh tokens</returns>
    /// <response code="200">OAuth authentication successful. Returns JWT tokens.</response>
    /// <response code="400">Invalid callback data or validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("oauth/callback")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleOAuthCallbackAsync(
        [FromBody] OAuthCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid OAuth callback request.");
        }

        _logger.LogInformation("Handling OAuth callback for provider: {Provider}", request.Provider);

        var result = await _authService.HandleOAuthCallbackAsync(request, cancellationToken);

        _logger.LogInformation("OAuth authentication successful for provider: {Provider}", request.Provider);

        return Success(result);
    }

    /// <summary>
    /// Redirects to Google OAuth authentication
    /// </summary>
    /// <remarks>
    /// Direct endpoint for Google OAuth authentication. This endpoint can be used
    /// for server-side redirects to initiate Google OAuth flow.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/auth/oauth/google?state=abc123&amp;return_url=https://myapp.com/callback
    /// 
    /// </remarks>
    /// <param name="state">CSRF protection state parameter</param>
    /// <param name="returnUrl">Optional return URL after authentication</param>
    /// <returns>Redirect to Google OAuth</returns>
    /// <response code="302">Redirect to Google OAuth authentication.</response>
    /// <response code="400">Invalid state parameter.</response>
    [HttpGet("oauth/google")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult RedirectToGoogle([FromQuery] string state, [FromQuery(Name = "return_url")] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return Invalid("State parameter is required.");
        }

        _logger.LogInformation("Redirecting to Google OAuth with state: {State}", state);

        // This would typically use the configured Google OAuth client
        // For now, return a placeholder redirect
        var googleOAuthUrl = $"https://accounts.google.com/oauth/authorize?client_id=YOUR_GOOGLE_CLIENT_ID&redirect_uri=YOUR_REDIRECT_URI&scope=email%20profile&response_type=code&state={state}";
        
        return Redirect(googleOAuthUrl);
    }

    /// <summary>
    /// Redirects to Facebook OAuth authentication
    /// </summary>
    /// <remarks>
    /// Direct endpoint for Facebook OAuth authentication. This endpoint can be used
    /// for server-side redirects to initiate Facebook OAuth flow.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/auth/oauth/facebook?state=abc123&amp;return_url=https://myapp.com/callback
    /// 
    /// </remarks>
    /// <param name="state">CSRF protection state parameter</param>
    /// <param name="returnUrl">Optional return URL after authentication</param>
    /// <returns>Redirect to Facebook OAuth</returns>
    /// <response code="302">Redirect to Facebook OAuth authentication.</response>
    /// <response code="400">Invalid state parameter.</response>
    [HttpGet("oauth/facebook")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult RedirectToFacebook([FromQuery] string state, [FromQuery(Name = "return_url")] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return Invalid("State parameter is required.");
        }

        _logger.LogInformation("Redirecting to Facebook OAuth with state: {State}", state);

        // This would typically use the configured Facebook OAuth client
        // For now, return a placeholder redirect
        var facebookOAuthUrl = $"https://www.facebook.com/v18.0/dialog/oauth?client_id=YOUR_FACEBOOK_APP_ID&redirect_uri=YOUR_REDIRECT_URI&scope=email&response_type=code&state={state}";
        
        return Redirect(facebookOAuthUrl);
    }
}

/// <summary>
/// Request model for refreshing tokens.
/// </summary>
public sealed record RefreshTokenRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    [Required]
    public required string RefreshToken { get; init; }
}

/// <summary>
/// Request model for password reset.
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}