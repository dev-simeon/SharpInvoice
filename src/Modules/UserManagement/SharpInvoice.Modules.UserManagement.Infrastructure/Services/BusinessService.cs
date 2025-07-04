namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Kernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SharpInvoice.Shared.Infrastructure.Persistence;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BusinessService(AppDbContext context, ICurrentUserProvider currentUserProvider, IFileStorageService fileStorageService) : IBusinessService
{
    public async Task<BusinessDto> CreateBusinessForUserAsync(Guid userId, string businessName, string userEmail, string country)
    {
        var business = Business.Create(businessName, userId, country);
        business.UpdateDetails(businessName, userEmail, null, null);

        var ownerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Owner");
        if (ownerRole == null)
        {
            ownerRole = Role.Create("Owner", "Full administrative access to the business.");
            // Seed all permissions for the owner role
            var allPermissions = await context.Permissions.ToListAsync();
            foreach (var perm in allPermissions)
            {
                ownerRole.AddPermission(perm);
            }
            context.Roles.Add(ownerRole);
        }

        var teamMember = TeamMember.Create(userId, business.Id, ownerRole.Id);
        context.Businesses.Add(business);
        context.TeamMembers.Add(teamMember);

        await context.SaveChangesAsync();

        return new BusinessDto(business.Id, business.Name);
    }

    public async Task<BusinessDetailsDto> GetBusinessDetailsAsync(Guid businessId)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        return new BusinessDetailsDto(
            business.Id, business.Name, business.Email, business.PhoneNumber, business.Website,
            business.Address, business.City, business.State, business.ZipCode, business.Country,
            business.LogoUrl, business.IsActive
        );
    }

    public async Task UpdateBusinessDetailsAsync(Guid businessId, UpdateBusinessDetailsDto dto)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        business.UpdateDetails(dto.Name, dto.Email, dto.PhoneNumber, dto.Website);
        business.UpdateAddress(dto.Address, dto.City, dto.State, dto.ZipCode, dto.Country ?? business.Country);
        business.UpdateBranding(dto.LogoUrl, JsonSerializer.Serialize(dto.ThemeSettings));

        await context.SaveChangesAsync();
    }

    public async Task UpdateBusinessLogoAsync(Guid businessId, Stream logoStream, string fileName)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (business.OwnerId != currentUserId)
        {
            throw new ForbidException("Only the business owner can update the logo.");
        }

        var logoUrl = await fileStorageService.SaveFileAsync(logoStream, fileName, "logos");
        business.UpdateBranding(logoUrl, business.ThemeSettings);

        await context.SaveChangesAsync();
    }

    public async Task<Guid> GetBusinessIdByOwnerAsync(Guid userId)
    {
        var business = await context.Businesses
            .FirstOrDefaultAsync(b => b.OwnerId == userId);

        return business?.Id ?? Guid.Empty;
    }

    public async Task<IEnumerable<BusinessDto>> GetBusinessesForUserAsync(Guid userId)
    {
        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (userId != currentUserId)
            throw new ForbidException("You can only view your own businesses.");

        return await context.Businesses
            .Where(b => b.OwnerId == userId || b.TeamMembers.Any(tm => tm.UserId == userId))
            .Select(b => new BusinessDto(b.Id, b.Name))
            .ToListAsync();
    }

    public async Task DeactivateBusinessAsync(Guid businessId)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can deactivate the business.");

        business.Deactivate();
        await context.SaveChangesAsync();
    }

    public async Task ActivateBusinessAsync(Guid businessId)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can activate the business.");

        business.Activate();
        await context.SaveChangesAsync();
    }
}