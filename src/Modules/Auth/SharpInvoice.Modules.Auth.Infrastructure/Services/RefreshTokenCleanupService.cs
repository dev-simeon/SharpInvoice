using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Infrastructure.Services
{
    /// <summary>
    /// Background service that periodically cleans up expired refresh tokens
    /// </summary>
    public class RefreshTokenCleanupService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RefreshTokenCleanupService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        private readonly ILogger<RefreshTokenCleanupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run once per day
        private readonly TimeSpan _startupDelay = TimeSpan.FromMinutes(5);  // Wait 5 minutes on startup before first run

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh Token Cleanup Service is starting");

            try
            {
                // Initial delay to prevent immediate execution during application startup
                _logger.LogInformation("Waiting {Delay} minutes before first cleanup cycle", _startupDelay.TotalMinutes);
                await Task.Delay(_startupDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Refresh Token Cleanup Service startup delay was canceled");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync();
                    _logger.LogInformation("Completed refresh token cleanup cycle");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during refresh token cleanup");
                }

                try
                {
                    // Wait for the next cleanup interval
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Refresh Token Cleanup Service was stopped");
                    break;
                }
            }
        }

        private async Task CleanupExpiredTokensAsync()
        {
            _logger.LogInformation("Starting expired refresh token cleanup");

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Find users with expired refresh tokens
            var now = DateTime.UtcNow;
            var usersWithExpiredTokens = await dbContext.Users
                .Where(u => u.RefreshTokenExpiryTime < now && u.RefreshToken != null)
                .ToListAsync();

            if (!usersWithExpiredTokens.Any())
            {
                _logger.LogInformation("No expired refresh tokens found");
                return;
            }

            // Clear expired tokens using domain method
            int count = 0;
            foreach (var user in usersWithExpiredTokens)
            {
                try
                {
                    // Use the domain method instead of directly setting properties
                    user.SetRefreshToken(string.Empty, DateTime.MinValue);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing token for user {UserId}", user.Id);
                }
            }

            try
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Cleared {Count} expired refresh tokens", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database during token cleanup");
            }
        }
    }
}