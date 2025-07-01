namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

public class UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor) : IUserService
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<UserDetailsDto> GetUserByIdAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        return new UserDetailsDto(user.Id, user.FirstName, user.LastName, user.Email);
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserDto userDto)
    {
        var currentUserId = GetCurrentUserId();
        if (userId != currentUserId)
        {
            throw new ForbidException("You can only update your own profile.");
        }

        var user = await context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        user.UpdateProfile(userDto.FirstName, userDto.LastName);

        await context.SaveChangesAsync();
    }
} 