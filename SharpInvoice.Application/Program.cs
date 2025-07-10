using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure;
using SharpInvoice.Infrastructure.Persistence;
using SharpInvoice.Infrastructure.Repositories;
using SharpInvoice.Infrastructure.Services;
using SharpInvoice.Infrastructure.Shared;
using System.Text;

// Configure a logger for application startup. This is separate from the main
// logger and is used to catch issues that occur before the host is fully built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SharpInvoice API host...");

    var builder = WebApplication.CreateBuilder(args);

    // A. Logging (Serilog)
    // Replace the default logging providers with Serilog.
    // The configuration is read from appsettings.json.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // B. Infrastructure Services (Database, Repositories, Services)
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // C. Authentication & Authorization
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
        ?? throw new InvalidOperationException("AppSettings is missing or invalid. Check your configuration.");
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddCookie(options =>
        {
            // This is essential for the external login flow to work
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = appSettings.Jwt.Issuer,
            ValidAudience = appSettings.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Secret))
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = appSettings.Authentication.Google.ClientId;
        options.ClientSecret = appSettings.Authentication.Google.ClientSecret;
    });

    // D. Add Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("database", () => {
            try {
                using var context = new AppDbContext(
                    new DbContextOptionsBuilder<AppDbContext>()
                        .UseSqlServer(appSettings.ConnectionStrings.SqlDbRemote)
                        .Options);
                context.Database.CanConnect();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex) {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer(async (document, context, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Sharp Invoice API",
                Version = "v1",
                Description = "Modern API for managing invoicing, payments, and user management.",
                Contact = new OpenApiContact
                {
                    Name = "SharpInvoice Support",
                    Email = "support@sharpinvoice.com",
                    Url = new Uri("https://api.sharpinvoice.com/support")
                }
            };
            await Task.CompletedTask;
        });
    });

    var app = builder.Build();
    
    app.UseSerilogRequestLogging();

    // E. Global Exception Handling
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                // Log the exception
                Log.Error(exceptionHandlerFeature.Error, "Unhandled exception");
                
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "An unexpected error occurred",
                    details = app.Environment.IsDevelopment() ? exceptionHandlerFeature.Error.Message : null
                });
            }
        });
    });

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/scalar/v1");
            return;
        }
        await next();
    });

    // F. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi("/api/{documentName}/openapi.json").CacheOutput();
    app.MapScalarApiReference("/scalar/v1", options =>
    {
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.OpenApiRoutePattern = "api/{documentName}/openapi.json";
        options.Theme = ScalarTheme.BluePlanet;
    });

    app.UseHttpsRedirection();

    // Map health check endpoint
    app.MapHealthChecks("/health");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SharpInvoice API host terminated unexpectedly.");
    if (ex.InnerException != null)
    {
        Log.Fatal(ex.InnerException, "Inner exception:");
    }
    throw; // Optionally rethrow to see the full stack in the console
}
finally
{
    Log.CloseAndFlush();
}