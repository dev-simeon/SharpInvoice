namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharpInvoice.Modules.Auth.Application.Commands;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System.Net.Mime;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.API.Examples;

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
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new user, hashes their password, and sets up an initial business for them.
    /// It then sends a confirmation email to verify the user's email address.
    /// </remarks>
    /// <param name="command">The registration details including email, password, and business information.</param>
    /// <returns>Registration response with user information.</returns>
    [HttpPost("register")]
    [EndpointName("Register User")]
    [EndpointSummary("Register a new user account")]
    [EndpointDescription("Creates a new user account with the provided information and sends a confirmation email")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(RegisterUserCommand), typeof(RegisterUserCommandExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(RegisterResponseDtoExample))]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await authService.RegisterAndCreateBusinessAsync(command);
        return Created($"api/user/me", result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT.
    /// </summary>
    /// <remarks>
    /// On successful authentication, if 2FA is not enabled, it returns JWT access and refresh tokens.
    /// If 2FA is enabled, it returns a challenge indicating a verification code has been sent.
    /// </remarks>
    /// <param name="command">The login credentials including email and password.</param>
    /// <returns>Authentication response with tokens or 2FA challenge.</returns>
    [HttpPost("login")]
    [EndpointName("User Login")]
    [EndpointSummary("Authenticate a user")]
    [EndpointDescription("Authenticates a user with credentials and returns JWT tokens or 2FA challenge")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [EnableRateLimiting("login")]
    [SwaggerRequestExample(typeof(LoginUserCommand), typeof(LoginUserCommandExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseDtoExample))]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await authService.LoginAsync(command);
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
    [EndpointName("Verify Two-Factor Authentication")]
    [EndpointSummary("Complete login with 2FA verification")]
    [EndpointDescription("Verifies the two-factor authentication code to complete the login process")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [EnableRateLimiting("login")]
    [SwaggerRequestExample(typeof(VerifyTwoFactorCommand), typeof(VerifyTwoFactorCommandExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseDtoExample))]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorCommand request)
    {
        var result = await authService.VerifyTwoFactorCodeAsync(request);
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
    [EndpointName("Initiate External Login")]
    [EndpointSummary("Start external authentication process")]
    [EndpointDescription("Redirects to an external authentication provider for login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalCallback), new { returnUrl })
            ?? throw new InvalidOperationException("Could not generate the external login callback URL.");
        var properties = authService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
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
    /// <param name="command">The command containing the user's email.</param>
    /// <returns>A confirmation message.</returns>
    [HttpPost("forgot-password")]
    [EndpointName("Forgot Password")]
    [EndpointSummary("Request password reset link")]
    [EndpointDescription("Sends a password reset link to the user's email address")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [EnableRateLimiting("login")]
    [SwaggerRequestExample(typeof(ForgotPasswordCommand), typeof(ForgotPasswordCommandExample))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        await authService.ForgotPasswordAsync(command);
        return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the user's password using a token.
    /// </summary>
    /// <remarks>
    /// The token is sent to the user's email and is required to change the password.
    /// </remarks>
    /// <param name="command">The command containing the new password, email, and reset token.</param>
    /// <returns>A success message or a bad request if the token is invalid.</returns>
    [HttpPost("reset-password")]
    [EndpointName("Reset Password")]
    [EndpointSummary("Reset user password")]
    [EndpointDescription("Changes a user's password using a valid reset token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [EnableRateLimiting("login")]
    [SwaggerRequestExample(typeof(ResetPasswordCommand), typeof(ResetPasswordCommandExample))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var success = await authService.ResetPasswordAsync(command);
        if (!success) return BadRequest(new { Message = "Invalid token or email." });
        return Ok(new { Message = "Password has been reset successfully." });
    }
}