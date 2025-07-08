namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
}