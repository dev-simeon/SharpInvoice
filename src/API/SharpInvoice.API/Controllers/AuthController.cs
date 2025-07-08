namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.API.Examples;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Provides endpoints for user authentication including registration, login, reset password, and token refresh.
/// </summary>
[ApiController]
[Tags("Authentication")]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new user and hashes their password.
    /// It then sends a confirmation email to verify the user's email address.
    /// After registration, the user should create a business.
    /// </remarks>
    /// <param name="request">The registration details including email and password.</param>
    /// <returns>A response containing the new user's ID.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EndpointName("Register User")]
    [EndpointSummary("Register a new user account")]
    [EndpointDescription("Creates a new user account with the provided information and sends a confirmation email")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegisterResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(RegisterUserRequest), typeof(RegisterUserCommandExample))]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var result = await authService.RegisterUserAsync(request.Email, request.FirstName, request.LastName, request.Password);
        return Created($"api/user/me", result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT.
    /// </summary>
    /// <remarks>
    /// On successful authentication, if 2FA is not enabled, it returns JWT access and refresh tokens.
    /// If 2FA is enabled, it returns a challenge indicating a verification code has been sent.
    /// </remarks>
    /// <param name="request">The login credentials including email and password.</param>
    /// <returns>Authentication response with tokens or 2FA challenge.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointName("User Login")]
    [EndpointSummary("Authenticate a user")]
    [EndpointDescription("Authenticates a user with credentials and returns JWT tokens or 2FA challenge")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(LoginUserRequest), typeof(LoginUserCommandExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseDtoExample))]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);
        if (result.IsTwoFactorRequired)
        {
            return Ok(new { result.IsTwoFactorRequired, result.Message });
        }
        return Ok(result.AuthResponse);
    }

    /// <summary>
    /// Verifies the two-factor code and completes the login process.
    /// </summary>
    /// <remarks>
    /// This endpoint is used after a successful login attempt where two-factor authentication was required.
    /// </remarks>
    /// <param name="request">The verification details including email and 2FA code.</param>
    /// <returns>Authentication response with tokens.</returns>
    [HttpPost("login/verify-two-factor")]
    [AllowAnonymous]
    [EndpointName("Verify Two-Factor Authentication")]
    [EndpointSummary("Complete login with 2FA verification")]
    [EndpointDescription("Verifies the two-factor authentication code to complete the login process")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(VerifyTwoFactorRequest), typeof(VerifyTwoFactorCommandExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseDtoExample))]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var result = await authService.VerifyTwoFactorAsync(request.Email, request.VerificationCode);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired JWT using a refresh token.
    /// </summary>
    /// <remarks>
    /// Uses a valid refresh token to generate a new JWT access token without requiring user credentials.
    /// </remarks>
    /// <param name="request">The refresh token details.</param>
    /// <returns>Authentication response with new tokens.</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EndpointName("Refresh Authentication Token")]
    [EndpointSummary("Refresh an expired JWT token")]
    [EndpointDescription("Uses a valid refresh token to generate a new JWT access token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(RefreshTokenRequest), typeof(RefreshTokenRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseDtoExample))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <remarks>
    /// This endpoint is typically triggered by a user clicking a link in a confirmation email.
    /// </remarks>
    /// <param name="userId">The ID of the user whose email is being confirmed.</param>
    /// <param name="token">The email confirmation token sent to the user's email.</param>
    /// <returns>Confirmation success message.</returns>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [EndpointName("Confirm User Email")]
    [EndpointSummary("Confirm a user's email address")]
    [EndpointDescription("Validates an email confirmation token to verify a user's email address")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var success = await authService.ConfirmEmailAsync(userId, token);
        if (!success) return BadRequest(new { Message = "Invalid email confirmation link." });
        return Ok(new { Message = "Email confirmed successfully." });
    }

    /// <summary>
    /// Initiates the external login process (e.g., for Google).
    /// </summary>
    /// <param name="provider">The external authentication provider (e.g., "Google" or "Facebook").</param>
    /// <param name="returnUrl">Optional. The URL to return to after successful authentication.</param>
    /// <returns>A challenge result that redirects to the external provider.</returns>
    [HttpGet("external-login")]
    [AllowAnonymous]
    [EndpointName("Initiate External Login")]
    [EndpointSummary("Start external authentication process")]
    [EndpointDescription("Redirects to an external authentication provider for login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalCallback), new { returnUrl })
            ?? throw new InvalidOperationException("Could not generate the external login callback URL.");
        var properties = ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from the external identity provider.
    /// </summary>
    /// <remarks>
    /// After the user authenticates with the external provider, they are redirected back to this endpoint,
    /// which completes the login or registration process.
    /// </remarks>
    /// <param name="returnUrl">Optional. The URL to redirect to after processing the callback.</param>
    /// <param name="remoteError">Optional. Error message from the external provider, if any.</param>
    /// <returns>Authentication response with tokens or error details.</returns>
    [HttpGet("external-callback")]
    [AllowAnonymous]
    [EndpointName("External Login Callback")]
    [EndpointSummary("Complete external authentication")]
    [EndpointDescription("Processes the callback from external authentication provider")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseDtoExample))]
    public async Task<IActionResult> ExternalCallback([FromQuery] string? returnUrl = null, [FromQuery] string? remoteError = null)
    {
        if (remoteError != null)
        {
            // Handle error, maybe redirect to a login failure page
            return BadRequest(new { Message = $"Error from external provider: {remoteError}" });
        }

        var result = await authService.HandleExternalLoginAsync();

        // Here you would typically redirect to the frontend with the tokens
        // For an API, we can return the tokens directly.
        return Ok(result);
    }

    /// <summary>
    /// Sends a password reset link to the user's email.
    /// </summary>
    /// <remarks>
    /// If a user with the specified email exists, an email with a password reset link is sent.
    /// For security reasons, the API returns a success message even if the email doesn't exist.
    /// </remarks>
    /// <param name="request">The request containing the user's email.</param>
    /// <returns>A confirmation message.</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EndpointName("Forgot Password")]
    [EndpointSummary("Request password reset link")]
    [EndpointDescription("Sends a password reset link to the user's email address")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [SwaggerRequestExample(typeof(ForgotPasswordRequest), typeof(ForgotPasswordCommandExample))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.SendPasswordResetEmailAsync(request.Email);
        return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the user's password using a token.
    /// </summary>
    /// <remarks>
    /// The token is sent to the user's email and is required to change the password.
    /// </remarks>
    /// <param name="request">The request containing the new password, email, and reset token.</param>
    /// <returns>A success message or a bad request if the token is invalid.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EndpointName("Reset Password")]
    [EndpointSummary("Reset user password")]
    [EndpointDescription("Changes a user's password using a valid reset token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(ResetPasswordRequest), typeof(ResetPasswordCommandExample))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (!success) return BadRequest(new { Message = "Invalid token or email." });
        return Ok(new { Message = "Password has been reset successfully." });
    }

    /// <summary>
    /// Logs out the user by revoking the refresh token
    /// </summary>
    /// <param name="request">The request containing the refresh token to revoke</param>
    /// <returns>Success or failure status</returns>
    [HttpPost("logout")]
    [Authorize]
    [EndpointName("Logout")]
    [EndpointSummary("Logout user and revoke tokens")]
    [EndpointDescription("Invalidates the refresh token to prevent further token refreshing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(RevokeTokenRequest), typeof(LogoutRequestExample))]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("Refresh token is required");
        }

        var result = await authService.LogoutAsync(request.RefreshToken);
        if (!result)
        {
            return BadRequest("Invalid token");
        }

        return Ok(new { message = "Successfully logged out" });
    }

    /// <summary>
    /// Gets all external accounts linked to the user's account
    /// </summary>
    /// <returns>A collection of linked external accounts</returns>
    [Authorize]
    [HttpGet("linked-accounts")]
    [EndpointName("Get Linked Accounts")]
    [EndpointSummary("Get user's linked external accounts")]
    [EndpointDescription("Returns a list of external identity providers linked to the user's account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLinkedAccounts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var linkedAccounts = await authService.GetLinkedAccountsAsync(userIdGuid);
        return Ok(linkedAccounts);
    }

    /// <summary>
    /// Links an external account to the user's account
    /// </summary>
    /// <param name="provider">The external provider (e.g. "Google", "Facebook")</param>
    /// <param name="returnUrl">Optional URL to return to after linking</param>
    /// <returns>Redirection to external provider</returns>
    [Authorize]
    [HttpGet("link-account")]
    [EndpointName("Link External Account")]
    [EndpointSummary("Link an external account")]
    [EndpointDescription("Starts the flow to link an external identity provider to the user's account")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult LinkAccount([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Store the information that this is an account linking flow (not a login)
        var redirectUrl = Url.Action(nameof(LinkAccountCallback), new { provider, returnUrl })
            ?? throw new InvalidOperationException("Could not generate the link callback URL.");

        var properties = ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Processes the callback from external provider during account linking
    /// </summary>
    /// <param name="provider">The external provider being linked</param>
    /// <param name="returnUrl">Optional URL to return to after linking</param>
    /// <param name="remoteError">Error from the external provider, if any</param>
    /// <returns>Result of the linking operation</returns>
    [Authorize]
    [HttpGet("link-account-callback")]
    [EndpointName("Link Account Callback")]
    [EndpointSummary("Complete account linking")]
    [EndpointDescription("Processes the callback from external provider during account linking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkAccountCallback(
        [FromQuery] string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? remoteError = null)
    {
        if (remoteError != null)
        {
            return BadRequest(new { Message = $"Error from external provider: {remoteError}" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var info = await HttpContext.AuthenticateAsync("Identity.External");
        if (info?.Principal == null)
        {
            return BadRequest(new { Message = "Could not retrieve external account information." });
        }

        var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(providerKey))
        {
            return BadRequest(new { Message = "External provider did not provide a unique identifier." });
        }

        var success = await authService.LinkAccountAsync(userIdGuid, provider, providerKey);

        if (!success)
        {
            return BadRequest(new { Message = "This account is already linked to another user." });
        }

        return Ok(new { Message = $"Successfully linked your {provider} account." });
    }

    /// <summary>
    /// Removes a linked external account from the user's account
    /// </summary>
    /// <param name="provider">The external provider to unlink</param>
    /// <returns>Result of the unlinking operation</returns>
    [Authorize]
    [HttpPost("unlink-account")]
    [EndpointName("Unlink External Account")]
    [EndpointSummary("Unlink an external account")]
    [EndpointDescription("Removes a link between the user's account and an external identity provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlinkAccount([FromQuery] string provider)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User identifier not found."));

        await authService.UnlinkAccountAsync(userId, provider);

        return Ok(new { Message = "Account unlinked successfully" });
    }

    private static AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
    {
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items[".Challenge"] = "true";
        properties.Items["LoginProvider"] = provider;
        return properties;
    }
}

// Request DTOs to replace MediatR commands
public record RegisterUserRequest(string Email, string FirstName, string LastName, string Password);
public record LoginUserRequest(string Email, string Password);
public record VerifyTwoFactorRequest(string Email, string VerificationCode);
public record RefreshTokenRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record RevokeTokenRequest(string RefreshToken);