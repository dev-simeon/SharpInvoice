namespace SharpInvoice.Modules.Auth.Application.Interfaces;

using SharpInvoice.Modules.Auth.Domain.Entities;

public interface IJwtTokenGenerator
{
    // Note: Added permissions to the signature for more granular authorization.
    string GenerateToken(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions);
}