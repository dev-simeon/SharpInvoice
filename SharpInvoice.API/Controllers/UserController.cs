using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for user profile management operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class UserController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="authService">The auth service.</param>
    /// <param name="logger">The logger.</param>
    public UserController(IAuthService authService, ILogger<UserController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    /// <remarks>
    /// Retrieves the authenticated user's profile information including personal details,
    /// preferences, and account settings.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/user/profile
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving profile for user: {UserId}", CurrentUserId);

        var user = await _authService.GetCurrentUserAsync(CurrentUserId!, cancellationToken);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Preferences = new UserPreferencesDto
            {
                Language = "en-US",
                TimeZone = "UTC",
                Currency = "USD",
                DateFormat = "MM/dd/yyyy",
                EmailNotifications = true,
                PushNotifications = false
            }
        };

        return Success(profile);
    }

    /// <summary>
    /// Updates the current user's profile
    /// </summary>
    /// <remarks>
    /// Updates the authenticated user's profile information including name, phone, and preferences.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/user/profile
    ///     {
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "phoneNumber": "+1-555-0123",
    ///         "preferences": {
    ///             "language": "en-US",
    ///             "timeZone": "America/New_York",
    ///             "currency": "USD",
    ///             "dateFormat": "MM/dd/yyyy",
    ///             "emailNotifications": true,
    ///             "pushNotifications": false
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The profile update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated profile information</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Invalid profile data.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="422">Validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid profile data provided.");
        }

        _logger.LogInformation("Updating profile for user: {UserId}", CurrentUserId);

        // In a real implementation, this would update the user profile in the database
        // For now, returning the updated profile based on the request
        var updatedProfile = new UserProfileDto
        {
            Id = CurrentUserId!,
            Email = CurrentUserEmail!,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            UpdatedAt = DateTime.UtcNow,
            Preferences = request.Preferences ?? new UserPreferencesDto()
        };

        return Success(updatedProfile);
    }

    /// <summary>
    /// Changes the user's password
    /// </summary>
    /// <remarks>
    /// Allows the user to change their password by providing the current password and a new password.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/user/change-password
    ///     {
    ///         "currentPassword": "CurrentPass123!",
    ///         "newPassword": "NewSecurePass456!",
    ///         "confirmPassword": "NewSecurePass456!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The password change data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password change confirmation</returns>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Invalid password data or passwords don't match.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Current password is incorrect.</response>
    /// <response code="422">New password doesn't meet security requirements.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid password change data provided.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return Invalid("New password and confirmation password do not match.");
        }

        _logger.LogInformation("Password change requested for user: {UserId}", CurrentUserId);

        // In a real implementation, this would:
        // 1. Verify the current password
        // 2. Validate the new password meets security requirements
        // 3. Hash and store the new password
        // 4. Invalidate existing refresh tokens
        
        return Success(new { message = "Password changed successfully." });
    }

    /// <summary>
    /// Uploads or updates the user's avatar
    /// </summary>
    /// <remarks>
    /// Uploads a new avatar image for the user. Supported formats: PNG, JPG, JPEG.
    /// Maximum file size: 5MB. Recommended dimensions: 400x400 pixels.
    /// 
    /// Sample request (multipart/form-data):
    /// 
    ///     POST /api/v1/user/avatar
    ///     Content-Type: multipart/form-data
    ///     Authorization: Bearer {your-access-token}
    ///     
    ///     [file upload data]
    /// 
    /// </remarks>
    /// <param name="avatarFile">The avatar image file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload confirmation with avatar URL</returns>
    /// <response code="200">Avatar uploaded successfully.</response>
    /// <response code="400">Invalid file format or size.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="413">File size too large.</response>
    /// <response code="415">Unsupported file format.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadAvatarAsync(
        [FromForm, Required] IFormFile avatarFile,
        CancellationToken cancellationToken = default)
    {
        if (avatarFile == null || avatarFile.Length == 0)
        {
            return Invalid("Avatar file is required.");
        }

        if (avatarFile.Length > 5 * 1024 * 1024) // 5MB
        {
            return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                new ProblemDetails
                {
                    Status = StatusCodes.Status413RequestEntityTooLarge,
                    Title = "File Too Large",
                    Detail = "Avatar file must be smaller than 5MB."
                });
        }

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg" };
        if (!allowedTypes.Contains(avatarFile.ContentType.ToLowerInvariant()))
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                new ProblemDetails
                {
                    Status = StatusCodes.Status415UnsupportedMediaType,
                    Title = "Unsupported File Type",
                    Detail = "Only PNG, JPG, and JPEG files are supported."
                });
        }

        _logger.LogInformation("Avatar upload requested for user: {UserId}, file size: {FileSize}",
            CurrentUserId, avatarFile.Length);

        // In a real implementation, this would upload to storage service
        var avatarUrl = $"/avatars/{CurrentUserId}/{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";

        return Success(new { AvatarUrl = avatarUrl, Message = "Avatar uploaded successfully." });
    }

    /// <summary>
    /// Deletes the user's account
    /// </summary>
    /// <remarks>
    /// Permanently deletes the user's account and all associated data.
    /// This action cannot be undone.
    /// 
    /// **Warning:** This is a destructive operation that cannot be reversed.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/user/account
    ///     {
    ///         "password": "UserPassword123!",
    ///         "confirmDeletion": "DELETE MY ACCOUNT"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Account deletion confirmation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="204">Account deleted successfully.</response>
    /// <response code="400">Invalid deletion request.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Password is incorrect or confirmation text doesn't match.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAccountAsync(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid account deletion request.");
        }

        if (request.ConfirmDeletion != "DELETE MY ACCOUNT")
        {
            return Forbidden("Confirmation text must exactly match 'DELETE MY ACCOUNT'.");
        }

        _logger.LogWarning("Account deletion requested for user: {UserId}", CurrentUserId);

        // In a real implementation, this would:
        // 1. Verify the password
        // 2. Soft delete or anonymize user data
        // 3. Remove user from all businesses/teams
        // 4. Cancel any subscriptions
        // 5. Schedule data purge

        return NoContent();
    }

    /// <summary>
    /// Gets user activity log
    /// </summary>
    /// <remarks>
    /// Retrieves the user's recent activity and login history.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/user/activity?page=1&pageSize=20
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User activity log</returns>
    /// <response code="200">Activity log retrieved successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(UserActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityLogAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Invalid("Page number must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Invalid("Page size must be between 1 and 100.");
        }

        _logger.LogDebug("Retrieving activity log for user: {UserId}, page: {Page}", CurrentUserId, page);

        // Generate mock activity data
        var activities = GenerateMockActivityData();
        var totalCount = activities.Count;
        var pagedActivities = activities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new UserActivityResponse
        {
            Activities = pagedActivities,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Success(response);
    }

    #region Private Helper Methods

    private static List<ActivityLogDto> GenerateMockActivityData()
    {
        return new List<ActivityLogDto>
        {
            new() { Action = "Login", Description = "User logged in", Timestamp = DateTime.UtcNow.AddHours(-1), IpAddress = "192.168.1.100" },
            new() { Action = "Invoice Created", Description = "Created invoice INV-001", Timestamp = DateTime.UtcNow.AddHours(-2), IpAddress = "192.168.1.100" },
            new() { Action = "Client Added", Description = "Added client Acme Corp", Timestamp = DateTime.UtcNow.AddHours(-3), IpAddress = "192.168.1.100" },
            new() { Action = "Profile Updated", Description = "Updated profile information", Timestamp = DateTime.UtcNow.AddDays(-1), IpAddress = "192.168.1.100" },
            new() { Action = "Password Changed", Description = "Changed account password", Timestamp = DateTime.UtcNow.AddDays(-2), IpAddress = "192.168.1.100" }
        };
    }

    #endregion
}

#region DTOs

/// <summary>
/// User profile data transfer object
/// </summary>
public sealed record UserProfileDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? AvatarUrl { get; init; }
    public UserPreferencesDto Preferences { get; init; } = new();
}

/// <summary>
/// User preferences data transfer object
/// </summary>
public sealed record UserPreferencesDto
{
    public string Language { get; init; } = "en-US";
    public string TimeZone { get; init; } = "UTC";
    public string Currency { get; init; } = "USD";
    public string DateFormat { get; init; } = "MM/dd/yyyy";
    public bool EmailNotifications { get; init; } = true;
    public bool PushNotifications { get; init; } = false;
}

/// <summary>
/// Update profile request data transfer object
/// </summary>
public sealed record UpdateProfileRequest
{
    /// <summary>
    /// User's first name
    /// </summary>
    /// <example>John</example>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; init; }

    /// <summary>
    /// User's last name
    /// </summary>
    /// <example>Doe</example>
    [Required]
    [StringLength(50)]
    public required string LastName { get; init; }

    /// <summary>
    /// Optional phone number
    /// </summary>
    /// <example>+1-555-0123</example>
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// User preferences
    /// </summary>
    public UserPreferencesDto? Preferences { get; init; }
}

/// <summary>
/// Change password request data transfer object
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    /// <example>CurrentPass123!</example>
    [Required]
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// New password
    /// </summary>
    /// <example>NewSecurePass456!</example>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string NewPassword { get; init; }

    /// <summary>
    /// Confirm new password
    /// </summary>
    /// <example>NewSecurePass456!</example>
    [Required]
    public required string ConfirmPassword { get; init; }
}

/// <summary>
/// Delete account request data transfer object
/// </summary>
public sealed record DeleteAccountRequest
{
    /// <summary>
    /// User's current password
    /// </summary>
    /// <example>UserPassword123!</example>
    [Required]
    public required string Password { get; init; }

    /// <summary>
    /// Confirmation text (must be exactly "DELETE MY ACCOUNT")
    /// </summary>
    /// <example>DELETE MY ACCOUNT</example>
    [Required]
    public required string ConfirmDeletion { get; init; }
}

/// <summary>
/// User activity response data transfer object
/// </summary>
public sealed record UserActivityResponse
{
    public List<ActivityLogDto> Activities { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Activity log data transfer object
/// </summary>
public sealed record ActivityLogDto
{
    public string Action { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

#endregion