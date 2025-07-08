namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBusinessRepository
{
    Task<Business?> GetByIdAsync(Guid id);
    Task<IEnumerable<Business>> GetByOwnerIdAsync(Guid ownerId);
    Task<Business?> GetByNameAndCountryAsync(string name, string country);
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task<IEnumerable<Business>> GetByUserIdAsync(Guid userId);
} 