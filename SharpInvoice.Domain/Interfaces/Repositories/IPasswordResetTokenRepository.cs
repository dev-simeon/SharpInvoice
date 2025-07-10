using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<IEnumerable<PasswordResetToken>> GetByUserEmailAsync(string userEmail);
    Task<bool> ExistsAsync(string id);
    Task AddAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task SaveChangesAsync();
} 