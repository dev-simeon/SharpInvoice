namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class BusinessService(
    IBusinessRepository businessRepository,
    ICurrentUserProvider currentUserProvider,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IBusinessService
{
    // Business Creation and Management
    public async Task<BusinessDto> CreateBusinessAsync(string name, string email, string country)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        
        var business = Business.Create(name, email, country, userId);
        await businessRepository.AddAsync(business);
        await unitOfWork.SaveChangesAsync();

        return new BusinessDto
        {
            Id = business.Id,
            Name = business.Name,
            Email = business.Email,
            Country = business.Country,
            IsActive = business.IsActive,
            CreatedAt = business.CreatedAt
        };
    }

    public async Task<BusinessDetailsDto> GetBusinessDetailsAsync(Guid businessId)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new InvalidOperationException("Business not found.");
        }

        return new BusinessDetailsDto
        {
            Id = business.Id,
            Name = business.Name,
            Email = business.Email,
            Country = business.Country,
            Address = business.Address,
            Phone = business.Phone,
            Website = business.Website,
            LogoUrl = business.LogoUrl,
            IsActive = business.IsActive,
            CreatedAt = business.CreatedAt
        };
    }

    public async Task<IEnumerable<BusinessDto>> GetMyBusinessesAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var businesses = await businessRepository.GetByOwnerIdAsync(userId);

        return businesses.Select(b => new BusinessDto
        {
            Id = b.Id,
            Name = b.Name,
            Email = b.Email,
            Country = b.Country,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt
        });
    }

    public async Task<Guid> GetBusinessIdByOwnerAsync(Guid ownerId)
    {
        var business = await businessRepository.GetFirstByOwnerIdAsync(ownerId);
        return business?.Id ?? Guid.Empty;
    }

    // Business Status Management
    public async Task ActivateBusinessAsync(Guid businessId)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new InvalidOperationException("Business not found.");
        }

        business.Activate();
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateBusinessAsync(Guid businessId)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new InvalidOperationException("Business not found.");
        }

        business.Deactivate();
        await unitOfWork.SaveChangesAsync();
    }

    // Business Details and Branding
    public async Task UpdateBusinessDetailsAsync(Guid businessId, UpdateBusinessDetailsDto dto)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new InvalidOperationException("Business not found.");
        }

        business.UpdateDetails(
            dto.Name,
            dto.Email,
            dto.Address,
            dto.Phone,
            dto.Website
        );

        await unitOfWork.SaveChangesAsync();
    }

    public async Task UploadBusinessLogoAsync(Guid businessId, Stream logoStream, string fileName)
    {
        var business = await businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new InvalidOperationException("Business not found.");
        }

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{businessId}_logo_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";

        // Store the file
        var logoUrl = await fileStorageService.SaveFileAsync(logoStream, "business-logos", uniqueFileName);

        // Update business with new logo URL
        business.UpdateLogo(logoUrl);
        await unitOfWork.SaveChangesAsync();
    }

    // Business Validation
    public async Task<bool> IsBusinessNameAvailableAsync(string name, string country)
    {
        return await businessRepository.IsNameAvailableAsync(name, country);
    }
}