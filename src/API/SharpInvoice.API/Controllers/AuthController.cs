namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.Auth.Application.Commands;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Net.Mime;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Provides endpoints for user authentication including registration, login, reset password, and token refresh.
/// </summary>
[ApiController]
[Tags("Authentication")]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary> Registers a new user and creates an associated business account. </summary>
    /// <remarks>This endpoint creates a new user, hashes their password, and sets up an initial business for them. It then sends a confirmation email.</remarks>
    [HttpPost("register")]
    [EndpointSummary("Register a new user and business.")]
    [EndpointDescription("Creates a user and an associated business, then sends a confirmation email.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var result = await authService.RegisterAndCreateBusinessAsync(command);
        return Ok(result);
    }

    /// <summary> Authenticates a user and returns a JWT or a 2FA challenge. </summary>
    /// <remarks>
    /// On successful authentication, if 2FA is not enabled, it returns JWT access and refresh tokens.
    /// If 2FA is enabled, it returns a challenge indicating a verification code has been sent.
    /// </remarks>
    [HttpPost("login")]
    [EndpointSummary("Authenticate a user.")]
    [EndpointDescription("Authenticates with email and password. May trigger a 2FA flow.")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        var result = await authService.LoginAsync(command);
        if (result.IsTwoFactorRequired)
        {
            return Ok(new { result.IsTwoFactorRequired, result.Message });
        }
        return Ok(result.AuthResponse);
    }

    /// <summary> Verifies the two-factor code and completes the login process. </summary>
    /// <remarks>This endpoint is used after a successful login attempt where two-factor authentication was required.</remarks>
    [HttpPost("login/verify-two-factor")]
    [EndpointSummary("Verify the two-factor code.")]
    [EndpointDescription("Verifies the time-sensitive code for two-factor authentication.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorCommand request)
    {
        var result = await authService.VerifyTwoFactorCodeAsync(request);
        return Ok(result);
    }

    /// <summary> Refreshes an authentication token. </summary>
    /// <remarks>Uses a valid refresh token to generate a new JWT access token without requiring user credentials.</remarks>
    [HttpPost("refresh-token")]
    [EndpointSummary("Refresh an expired access token.")]
    [EndpointDescription("Generates a new JWT access token using a valid refresh token.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary> Confirms a user's email address using a token. </summary>
    /// <remarks>This endpoint is typically triggered by a user clicking a link in a confirmation email.</remarks>
    [HttpGet("confirm-email")]
    [EndpointSummary("Confirm a user's email address.")]
    [EndpointDescription("Validates the email confirmation token and marks the user's email as confirmed.")]
    [ProducesResponseType(typeof(OkObjectResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var success = await authService.ConfirmEmailAsync(userId, token);
        if (!success) return BadRequest(new { Message = "Invalid email confirmation link." });
        return Ok(new { Message = "Email confirmed successfully." });
    }

    /// <summary>
    /// Initiates an external authentication flow.
    /// </summary>
    /// <param name="provider">The external authentication provider (e.g., "Google" or "Facebook").</param>
    /// <param name="returnUrl">The URL to return to after successful authentication.</param>
    [HttpGet("external-login")]
    [EndpointDescription("Redirects to an external provider (Google, Facebook) for authentication.")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalCallback), new { returnUrl })
            ?? throw new InvalidOperationException("Could not generate the external login callback URL.");
        var properties = authService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from an external authentication provider.
    /// </summary>
    /// <remarks>After the user authenticates with the external provider, they are redirected back to this endpoint, which completes the login or registration process.</remarks>
    [HttpGet("external-callback")]
    [EndpointDescription("Handles the authentication callback from the external provider.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = null, string? remoteError = null)
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
    /// Initiates the password reset process for a user.
    /// </summary>
    /// <remarks>If a user with the specified email exists, an email with a password reset link is sent.</remarks>
    /// <param name="command">The command containing the user's email.</param>
    /// <returns>A confirmation message.</returns>
    [HttpPost("forgot-password")]
    [EndpointDescription("Sends a password reset link to the user's email.")]
    [ProducesResponseType(typeof(OkObjectResult), StatusCodes.Status200OK)]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        await authService.ForgotPasswordAsync(command);
        return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    /// <remarks>The token is sent to the user's email and is required to change the password.</remarks>
    /// <param name="command">The command containing the new password and reset token.</param>
    /// <returns>A success message or a bad request if the token is invalid.</returns>
    [HttpPost("reset-password")]
    [EndpointDescription("Sets a new password using a valid reset token.")]
    [ProducesResponseType(typeof(OkObjectResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        var success = await authService.ResetPasswordAsync(command);
        if (!success) return BadRequest(new { Message = "Invalid token or email." });
        return Ok(new { Message = "Password has been reset successfully." });
    }
}