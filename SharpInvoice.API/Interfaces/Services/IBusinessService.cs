using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

/// <summary>
/// Defines the contract for business-related operations.
/// </summary>
public interface IBusinessService
{
    /// <summary>
    /// Creates a new business for a specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user creating the business.</param>
    /// <param name="createBusinessRequest">The business creation request data.</param>
    /// <returns>The newly created business as a DTO.</returns>
    Task<BusinessDto> CreateBusinessAsync(string userId, CreateBusinessRequest createBusinessRequest);

    /// <summary>
    /// Retrieves a business by its unique identifier.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business.</param>
    /// <returns>The business DTO, or null if not found.</returns>
    Task<BusinessDto?> GetBusinessByIdAsync(Guid businessId);

    /// <summary>
    /// Updates an existing business.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business to update.</param>
    /// <param name="updateBusinessRequest">The business update request data.</param>
    /// <returns>The updated business as a DTO.</returns>
    Task<BusinessDto> UpdateBusinessAsync(Guid businessId, CreateBusinessRequest updateBusinessRequest);

    /// <summary>
    /// Deletes a business by its unique identifier.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteBusinessAsync(Guid businessId);

    /// <summary>
    /// Uploads a logo for the specified business.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business.</param>
    /// <param name="logoFile">The logo file to upload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL of the uploaded logo.</returns>
    Task<string> UploadBusinessLogoAsync(Guid businessId, IFormFile logoFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates business branding and theme settings.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business.</param>
    /// <param name="request">The branding update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated business as a DTO.</returns>
    Task<BusinessDto> UpdateBusinessBrandingAsync(Guid businessId, UpdateBusinessBrandingRequest request, CancellationToken cancellationToken = default);
} 