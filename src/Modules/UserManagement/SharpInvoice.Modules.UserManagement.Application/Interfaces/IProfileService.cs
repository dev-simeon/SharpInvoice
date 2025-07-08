namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System.Threading.Tasks;

/// <summary>
/// Comprehensive profile service interface that handles all user profile operations
/// </summary>
public interface IProfileService
{
    // Profile Management
    Task<ProfileDto> GetMyProfileAsync();
    Task UpdateMyProfileAsync(UpdateProfileDto profileDto);
}