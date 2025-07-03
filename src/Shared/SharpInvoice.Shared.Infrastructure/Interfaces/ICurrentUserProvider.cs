namespace SharpInvoice.Shared.Infrastructure.Interfaces;

using System;

/// <summary>
/// Defines methods to access information about the currently authenticated user.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Gets the ID of the currently authenticated user.
    /// </summary>
    /// <returns>The user's ID as a GUID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    Guid GetCurrentUserId();

    /// <summary>
    /// Tries to get the ID of the currently authenticated user.
    /// </summary>
    /// <param name="userId">When this method returns, contains the user's ID if the user is authenticated, or Guid.Empty otherwise.</param>
    /// <returns>true if the user is authenticated and has a valid user ID; otherwise, false.</returns>
    bool TryGetCurrentUserId(out Guid userId);

    /// <summary>
    /// Gets the business ID associated with the currently authenticated user.
    /// </summary>
    /// <returns>The business ID from claims, or Guid.Empty if not found.</returns>
    Guid GetCurrentBusinessId();
} 