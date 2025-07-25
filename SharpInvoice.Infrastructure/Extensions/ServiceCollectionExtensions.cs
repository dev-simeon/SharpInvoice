using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Options;
using SharpInvoice.Infrastructure.Persistence;
using SharpInvoice.Infrastructure.Repositories;
using SharpInvoice.Infrastructure.Services;

namespace SharpInvoice.Infrastructure.Extensions;

/// <summary>
/// Extension methods for setting up services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the infrastructure layer services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to use for retrieving app settings.</param>
    /// <returns>The service collection with added services.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AppSettings
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new InvalidOperationException("AppSettings is missing or invalid. Check your configuration.");
        services.AddSingleton(appSettings);

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)
            ).EnableSensitiveDataLogging(configuration.GetValue<bool>("EnableSensitiveDataLogging", false))
             .EnableDetailedErrors());

        // Register Repositories
        services.AddRepositories();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Infrastructure Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICachingService, CachingService>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        
        // Register Domain Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IStorageService>(provider => 
            new StorageService(configuration.GetConnectionString("StorageConnection")));

        return services;
    }

    /// <summary>
    /// Adds the repository services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The service collection to add repositories to.</param>
    /// <returns>The service collection with added repositories.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
} 