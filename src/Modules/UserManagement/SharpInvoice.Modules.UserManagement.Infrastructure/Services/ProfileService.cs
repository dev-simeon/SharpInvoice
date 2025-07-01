namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharpInvoice.Shared.Kernel.Exceptions;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        return new ProfileDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.AvatarUrl,
            user.PhoneNumber
        );
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        user.UpdateProfile(dto.FirstName, dto.LastName);
        user.UpdateContactInfo(dto.AvatarUrl, dto.PhoneNumber);

        await _context.SaveChangesAsync();
    }
} 