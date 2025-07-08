namespace SharpInvoice.Shared.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Security.Claims;

/// <summary>
/// Provides methods to access information about the currently authenticated user.
/// </summary>
public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    /// <summary>
    /// Gets the ID of the currently authenticated user.
    /// </summary>
    /// <returns>The user's ID as a GUID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    public Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    /// <summary>
    /// Tries to get the ID of the currently authenticated user.
    /// </summary>
    /// <param name="userId">When this method returns, contains the user's ID if the user is authenticated, or Guid.Empty otherwise.</param>
    /// <returns>true if the user is authenticated and has a valid user ID; otherwise, false.</returns>
    public bool TryGetCurrentUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdString = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out userId);
    }

    /// <summary>
    /// Gets the business ID associated with the currently authenticated user.
    /// </summary>
    /// <returns>The business ID from claims, or Guid.Empty if not found.</returns>
    public Guid GetCurrentBusinessId()
    {
        var businessId = httpContextAccessor.HttpContext?.User?.FindFirstValue("business_id");
        return Guid.TryParse(businessId, out var id) ? id : Guid.Empty;
    }
}