namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.Auth.Domain.Entities;
using System;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
} 