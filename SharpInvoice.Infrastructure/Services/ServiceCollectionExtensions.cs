using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Persistence;
using SharpInvoice.Infrastructure.Repositories;
using SharpInvoice.Infrastructure.Shared;

namespace SharpInvoice.Infrastructure.Services;

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
                appSettings.ConnectionStrings.SqlDbLocal,
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)
            ).EnableSensitiveDataLogging()
             .EnableDetailedErrors());

        // Register Repositories
        services.AddRepositories();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Services
        services.AddApplicationServices();

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

    /// <summary>
    /// Adds the application services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection with added services.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

        return services;
    }
} 