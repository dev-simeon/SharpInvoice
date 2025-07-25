using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharpInvoice.API.Dtos;
using SharpInvoice.API.Infrastructure.Exceptions;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;

namespace SharpInvoice.API.Services;

/// <summary>
/// Provides services for managing business entities.
/// </summary>
public class BusinessService : IBusinessService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ILogger<BusinessService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for database operations.</param>
    /// <param name="storageService">The storage service for file operations.</param>
    /// <param name="logger">The logger.</param>
    public BusinessService(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ILogger<BusinessService> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new business and assigns the creator as the owner.
    /// </summary>
    /// <param name="userId">The ID of the user creating the business.</param>
    /// <param name="createBusinessRequest">The business creation request data.</param>
    /// <returns>The created business as a DTO.</returns>
    public async Task<BusinessDto> CreateBusinessAsync(string userId, CreateBusinessRequest createBusinessRequest, CancellationToken cancellationToken = default)
    {
        var business = new Business(
            createBusinessRequest.Name,
            userId,
            createBusinessRequest.Country
        );

        // Update additional business information
        business.UpdateDetails(
            createBusinessRequest.Name,
            createBusinessRequest.Email,
            createBusinessRequest.PhoneNumber,
            createBusinessRequest.Website
        );
        business.UpdateAddress(
            createBusinessRequest.Address,
            createBusinessRequest.City,
            createBusinessRequest.State,
            createBusinessRequest.ZipCode,
            createBusinessRequest.Country
        );

        var teamMember = new TeamMember(
            userId,
            business.Id,
            BusinessRole.Owner.ToString()
        );

        await _unitOfWork.Businesses.AddAsync(business);
        await _unitOfWork.TeamMembers.AddAsync(teamMember);
        await _unitOfWork.SaveChangesAsync();

        return ToBusinessDto(business);
    }

    /// <summary>
    /// Deletes a business by its ID.
    /// </summary>
    /// <param name="businessId">The ID of the business to delete.</param>
    public async Task DeleteBusinessAsync(string businessId, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
        if (business != null)
        {
            await _unitOfWork.Businesses.RemoveAsync(business);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves a business by its ID.
    /// </summary>
    /// <param name="businessId">The ID of the business to retrieve.</param>
    /// <returns>The business DTO, or null if not found.</returns>
    public async Task<BusinessDto?> GetBusinessByIdAsync(string businessId, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
        return business == null ? null : ToBusinessDto(business);
    }

    /// <summary>
    /// Updates an existing business.
    /// </summary>
    /// <param name="businessId">The ID of the business to update.</param>
    /// <param name="updateBusinessRequest">The business update request data.</param>
    /// <returns>The updated business DTO.</returns>
    public async Task<BusinessDto> UpdateBusinessAsync(string businessId, CreateBusinessRequest updateBusinessRequest, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
        if (business == null)
        {
            // Or throw a custom not found exception
            throw new Exception("Business not found");
        }

        business.UpdateDetails(
            updateBusinessRequest.Name,
            updateBusinessRequest.Email,
            updateBusinessRequest.PhoneNumber,
            updateBusinessRequest.Website
        );
        business.UpdateAddress(
            updateBusinessRequest.Address,
            updateBusinessRequest.City,
            updateBusinessRequest.State,
            updateBusinessRequest.ZipCode,
            updateBusinessRequest.Country
        );

        await _unitOfWork.Businesses.UpdateAsync(business);
        await _unitOfWork.SaveChangesAsync();

        return ToBusinessDto(business);
    }
    
    /// <summary>
    /// Uploads a logo for the specified business.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business.</param>
    /// <param name="logoFile">The logo file to upload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL of the uploaded logo.</returns>
    public async Task<string> UploadBusinessLogoAsync(string businessId, IFormFile logoFile, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uploading logo for business: {BusinessId}", businessId);

            var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken)
                ?? throw new NotFoundException($"Business with ID {businessId} not found");

            // Generate unique filename
            var fileExtension = Path.GetExtension(logoFile.FileName);
            var fileName = $"business-{businessId}-logo-{Guid.NewGuid()}{fileExtension}";

            // Upload file to storage
            var logoUrl = await _storageService.UploadFileAsync("business-logos", fileName, logoFile.OpenReadStream());

            // Update business with logo URL
            business.UpdateBranding(logoUrl, business.ThemeSettings);
            await _unitOfWork.Businesses.UpdateAsync(business);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Logo uploaded successfully for business: {BusinessId}, URL: {LogoUrl}", businessId, logoUrl);

            return logoUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for business: {BusinessId}", businessId);
            throw;
        }
    }

    /// <summary>
    /// Updates business branding and theme settings.
    /// </summary>
    /// <param name="businessId">The unique identifier of the business.</param>
    /// <param name="request">The branding update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated business as a DTO.</returns>
    public async Task<BusinessDto> UpdateBusinessBrandingAsync(string businessId, UpdateBusinessBrandingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating branding for business: {BusinessId}", businessId);

            var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken)
                ?? throw new NotFoundException($"Business with ID {businessId} not found");

            // Convert theme settings to JSON
            var themeSettingsJson = request.ThemeSettings.ToJson();

            // Update business branding
            business.UpdateBranding(request.LogoUrl, themeSettingsJson);
            await _unitOfWork.Businesses.UpdateAsync(business);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Branding updated successfully for business: {BusinessId}", businessId);

            return ToBusinessDto(business);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branding for business: {BusinessId}", businessId);
            throw;
        }
    }

    /// <summary>
    /// Maps a Business entity to a BusinessDto.
    /// </summary>
    /// <param name="business">The business entity.</param>
    /// <returns>The mapped business DTO.</returns>
    private BusinessDto ToBusinessDto(Business business)
    {
        return new BusinessDto
        {
            Id = business.Id,
            Name = business.Name,
            Address = business.Address,
            City = business.City,
            State = business.State,
            Country = business.Country,
            ZipCode = business.ZipCode,
            PhoneNumber = business.PhoneNumber,
            Email = business.Email,
            Website = business.Website,
            CreatedAt = business.CreatedAt
        };
    }

    public async Task<IEnumerable<BusinessDto>> GetBusinessesByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var teamMembers = await _unitOfWork.TeamMembers.FindAsync(tm => tm.UserId == userId);
        var businessIds = teamMembers.Select(tm => tm.BusinessId).Distinct();
        
        var businesses = new List<BusinessDto>();
        foreach (var businessId in businessIds)
        {
            var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
            if (business != null)
            {
                businesses.Add(ToBusinessDto(business));
            }
        }
        
        return businesses;
    }

    public async Task<BusinessStatsDto> GetBusinessStatsAsync(string businessId, string period, CancellationToken cancellationToken = default)
    {
        var startDate = period.ToLower() switch
        {
            "month" => DateTime.UtcNow.AddMonths(-1),
            "quarter" => DateTime.UtcNow.AddMonths(-3),
            "year" => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.UtcNow.AddMonths(-1)
        };

        var invoices = await _unitOfWork.Invoices.FindAsync(i => i.BusinessId == businessId && i.CreatedAt >= startDate);
        var clients = await _unitOfWork.Clients.FindAsync(c => c.BusinessId == businessId && c.CreatedAt >= startDate);

        var totalRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total);
        var totalOutstanding = invoices.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.Total);
        var invoicesCreated = invoices.Count();
        var invoicesPaid = invoices.Count(i => i.Status == InvoiceStatus.Paid);
        var newClients = clients.Count();
        var averageInvoiceAmount = invoicesCreated > 0 ? invoices.Average(i => i.Total) : 0;

        return new BusinessStatsDto
        {
            TotalRevenue = totalRevenue,
            TotalOutstanding = totalOutstanding,
            InvoicesCreated = invoicesCreated,
            InvoicesPaid = invoicesPaid,
            NewClients = newClients,
            AverageInvoiceAmount = averageInvoiceAmount,
            TopClients = [],
            MonthlyRevenue = [],
            PaymentMethods = []
        };
    }
} 