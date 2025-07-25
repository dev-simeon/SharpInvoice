using FluentValidation;
using SharpInvoice.API.Services;
using System.Reflection;
using SharpInvoice.API.Middleware.Exceptions;
using SharpInvoice.API.Interfaces.Services;

namespace SharpInvoice.API.Middleware;

/// <summary>
/// Extension methods for setting up API services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the API layer services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection with added services.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Controllers
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        // Configure API Explorer for OpenAPI
        services.AddEndpointsApiExplorer();

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();

        // Register Global Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }
}