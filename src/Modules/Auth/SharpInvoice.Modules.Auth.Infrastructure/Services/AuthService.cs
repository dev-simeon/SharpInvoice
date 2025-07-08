namespace SharpInvoice.Modules.Auth.Infrastructure.Services;

using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AuthService(IJwtTokenGenerator tokenGenerator, IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponseDto> GenerateTokensAsync(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion)
    {
        var token = tokenGenerator.GenerateToken(user, businessId, roles, permissions);
        var refreshToken = user.GenerateRefreshToken();
        await unitOfWork.SaveChangesAsync();
        return new AuthResponseDto(user.Id.ToString(), user.Email, token, refreshToken, requiresProfileCompletion);
    }
}