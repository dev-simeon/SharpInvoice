using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using SharpInvoice.Infrastructure.Persistence;
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


    // B. Application Settings
    // Bind the "AppSettings" section from configuration to the AppSettings POCO class.
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
        ?? throw new InvalidOperationException("AppSettings is missing or invalid. Check your configuration.");

    // Register the AppSettings instance as a singleton for dependency injection.
    builder.Services.AddSingleton(appSettings);


    // C. Database Contexts
    // Register the DbContext for Entity Framework Core.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            appSettings.ConnectionStrings.SqlDbRemote,
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null)
        ).EnableSensitiveDataLogging()
         .EnableDetailedErrors()
         .LogTo(Console.WriteLine, LogLevel.Debug));

    // Add services to the container.
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
    // .AddFacebook(options =>
    // {
    //     options.AppId = appSettings.Authentication.Facebook.AppId;
    //     options.AppSecret = appSettings.Authentication.Facebook.AppSecret;
    // });
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

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/scalar/v1");
            return;
        }
        await next();
    });

    // I. Authentication & Authorization
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

    app.UseAuthorization();

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