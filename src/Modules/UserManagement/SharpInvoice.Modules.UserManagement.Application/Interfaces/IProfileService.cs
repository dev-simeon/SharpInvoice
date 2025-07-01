namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;
using System.Threading.Tasks;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}