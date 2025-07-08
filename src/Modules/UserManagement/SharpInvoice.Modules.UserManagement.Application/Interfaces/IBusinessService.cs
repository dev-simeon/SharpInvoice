namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Comprehensive business service interface that handles all business management operations
/// </summary>
public interface IBusinessService
{
    // Business Creation and Management
    Task<BusinessDto> CreateBusinessAsync(string name, string email, string country);
    Task<BusinessDetailsDto> GetBusinessDetailsAsync(Guid businessId);
    Task<IEnumerable<BusinessDto>> GetMyBusinessesAsync();
    Task<Guid> GetBusinessIdByOwnerAsync(Guid ownerId);
    
    // Business Status Management
    Task ActivateBusinessAsync(Guid businessId);
    Task DeactivateBusinessAsync(Guid businessId);
    
    // Business Details and Branding
    Task UpdateBusinessDetailsAsync(Guid businessId, UpdateBusinessDetailsDto dto);
    Task UploadBusinessLogoAsync(Guid businessId, Stream logoStream, string fileName);
    
    // Business Validation
    Task<bool> IsBusinessNameAvailableAsync(string name, string country);
}