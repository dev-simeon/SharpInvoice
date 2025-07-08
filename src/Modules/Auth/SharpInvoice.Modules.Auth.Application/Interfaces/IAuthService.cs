namespace SharpInvoice.Modules.Auth.Application.Interfaces;

using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAuthService
{
    Task<AuthResponseDto> GenerateTokensAsync(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion);
}