namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

public interface IBusinessService
{
    Task<BusinessDto> CreateBusinessForUserAsync(Guid userId, string businessName, string userEmail, string country);
    Task<BusinessDetailsDto> GetBusinessDetailsAsync(Guid businessId);
    Task<IEnumerable<BusinessDto>> GetBusinessesForUserAsync(Guid userId);
    Task UpdateBusinessDetailsAsync(Guid businessId, UpdateBusinessDetailsDto dto);
    Task UpdateBusinessLogoAsync(Guid businessId, Stream logoStream, string fileName);
    Task<Guid> GetBusinessIdByOwnerAsync(Guid userId);
    Task DeactivateBusinessAsync(Guid businessId);
    Task ActivateBusinessAsync(Guid businessId);
}
