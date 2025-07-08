namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

public class ProfileService(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider,
    IUnitOfWork unitOfWork) : IProfileService
{
    // Profile Management
    public async Task<ProfileDto> GetMyProfileAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return new ProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            IsProfileCompleted = user.IsProfileCompleted,
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateMyProfileAsync(UpdateProfileDto profileDto)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.UpdateProfile(
            profileDto.FirstName,
            profileDto.LastName,
            profileDto.PhoneNumber,
            profileDto.Address,
            profileDto.City,
            profileDto.Country,
            profileDto.PostalCode
        );

        await unitOfWork.SaveChangesAsync();
    }
}