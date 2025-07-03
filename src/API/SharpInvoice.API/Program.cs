// This is the main entry point of the application.
// It sets up the web server and the request processing pipeline.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Scalar.AspNetCore;
using SendGrid.Extensions.DependencyInjection;
using Serilog;
using SharpInvoice.API.Configuration;
using SharpInvoice.API.ErrorHandling;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Infrastructure.Services;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Infrastructure.Services;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Infrastructure.Persistence;
using SharpInvoice.Shared.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;
using Swashbuckle.AspNetCore.Filters;

// --- 1. BOOTSTRAP SERILOG ---
// Configure a logger for application startup. This is separate from the main
// logger and is used to catch issues that occur before the host is fully built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SharpInvoice API host...");

    var builder = WebApplication.CreateBuilder(args);

    // --- 2. CONFIGURE SERVICES ---

    // A. Logging (Serilog)
    // Replace the default logging providers with Serilog.
    // The configuration is read from appsettings.json.
    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    // B. Application Settings
    // Bind the AppSettings section from configuration and register it as a singleton.
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
        ?? throw new InvalidOperationException("Could not bind AppSettings from configuration.");
    builder.Services.AddSingleton(appSettings);

    // C. Database Contexts
    // Register the DbContext for Entity Framework Core.
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        appSettings.ConnenctionStrings.SqlDbLocal,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    ).EnableSensitiveDataLogging()
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, LogLevel.Debug));

    // D. Application Services
    // Register application-specific services for dependency injection.
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBusinessService, BusinessService>();
    builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
    builder.Services.AddScoped<IFileStorageService, LocalStorageService>();
    builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

    if (builder.Environment.IsDevelopment())
    {
        // Use Mailtrap for local development and testing
        var mailtrapSettings = appSettings.Mailtrap;
        builder.Services.AddHttpClient<IEmailSender, MailtrapEmailSender>(client =>
        {
            client.BaseAddress = new Uri("https://sandbox.api.mailtrap.io/");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Api-Token", mailtrapSettings.ApiKey);
        });
    }
    else
    {
        // Use SendGrid for production
        builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();
        builder.Services.AddSendGrid(options => options.ApiKey = appSettings.SendGrid.ApiKey);
    }

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthenticationCore();

    // E. Error Handling
    // Register custom exception handlers. They are executed in the order they are registered.
    builder.Services.AddExceptionHandler<ForbidExceptionHandler>();
    builder.Services.AddExceptionHandler<InvalidTokenExceptionHandler>();
    builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddSingleton<ProblemDetailsFactory, ValidationProblemDetailsFactory>();
    
    // Configure validation behavior for automatic 400 responses
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = false; // Enable automatic 400 responses
        options.InvalidModelStateResponseFactory = context =>
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = factory.CreateValidationProblemDetails(
                context.HttpContext,
                context.ModelState,
                statusCode: StatusCodes.Status400BadRequest,
                title: "One or more validation errors occurred.");
                
            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

    // H. Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // F. Authentication
    // Configure JWT bearer and external authentication providers (Google, Facebook).
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key))
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

    // G. Core ASP.NET Services
    // Add essential services for controllers, API exploration, and caching.
    builder.Services.AddControllers()
                      .AddNewtonsoftJson(options =>
                      {
                          options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                          options.SerializerSettings.Converters.Add(new StringEnumConverter());
                      });
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<SharpInvoice.API.Validators.Auth.RegisterUserCommandValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<SharpInvoice.API.Validators.UserManagement.UpdateProfileDtoValidator>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMemoryCache();
    builder.Services.AddResponseCompression();
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // H. API Documentation (OpenAPI / Scalar)
    // Configure OpenAPI and Scalar for API documentation.
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

    // Add Swagger examples
    builder.Services.AddSwaggerGen(c => 
    {
        c.ExampleFilters();
    });
    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    // CORS: Allow any origin for development, restrict in production
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });
    }
    else
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ProdCors", policy =>
                policy.WithOrigins("https://your-production-domain.com")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });
    }

    // --- 3. BUILD THE APPLICATION ---
    var app = builder.Build();

    // --- 4. CONFIGURE THE HTTP REQUEST PIPELINE ---
    // The order of middleware registration is critical.

    // A. Error Handling Middleware
    app.UseExceptionHandler();

    //// B. Security Headers (CSP, HSTS, etc.)
    //app.UseSecurityHeaders(policy =>
    //    policy.AddDefaultSecurityHeaders()
    //          .AddStrictTransportSecurity(maxAgeInSeconds: 31536000, includeSubdomains: true, preload: false)
    //          .AddContentSecurityPolicy(builder =>
    //          {
    //              builder.AddDefaultSrc().Self();
    //              builder.AddStyleSrc()
    //                  .Self()
    //                  .WithHash("sha256", "efbUoiQm/B89WfPrCw5bS46IRtEEzIuQGeB2ScOtNrY=")
    //                  .WithHash("sha256", "S/LcxN1zzSmlOmrB0jLtdk+Hf++yrpPaqh/zDyIP1n0=")
    //                  .WithHash("sha256", "WOQJAfGFwlXJxngayYmMNTqx4exn6yDCYB91U+VuHjI=")
    //                  .WithHash("sha256", "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU="); // Added missing style hash
    //              builder.AddScriptSrc()
    //                  .Self()
    //                  .WithHash("sha256", "88fCEs/foNf6+dc522ysL/kjlXW6d03HYxoC8/UPpQQ=")
    //                  .WithHash("sha256", "oGikWC/2m6b6vzQGwFcwl9O+EaY3f2K95Acoaf8/Gyc=")
    //                  .WithHash("sha256", "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=")
    //                  .WithHash("sha256", "MVK6K6c1DHv9C+pvJqNgs6vELb5hkGblw90JjvBkYmQ=")
    //                  .WithHash("sha256", "FPyEdp9GEPaFwIcUuWHYinwPMaAq8/sjLYLqi07vp40="); // Added missing script hash
    //              var connectSrc = builder.AddConnectSrc().Self().Sources;
    //              connectSrc.Add("ws://localhost:*");
    //              connectSrc.Add("wss://localhost:*");
    //              connectSrc.Add("http://localhost:*");
    //              connectSrc.Add("https://localhost:*");
    //          }));

    // C. CORS
    if (app.Environment.IsDevelopment())
        app.UseCors("DevCors");
    else
        app.UseCors("ProdCors");

    // D. HTTPS redirection
    app.UseHttpsRedirection();

    // E. Response Compression
    app.UseResponseCompression();

    // F. Logging
    app.UseSerilogRequestLogging();

    // G. Rate Limiting
    app.UseRateLimiter();

    // H. Root path redirect
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

    // J. API Documentation Endpoints
    app.MapOpenApi("/api/{documentName}/openapi.json").CacheOutput();
    app.MapScalarApiReference("/scalar/v1", options =>
    {
        options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Axios);
        options.OpenApiRoutePattern = "api/{documentName}/openapi.json";
        options.Theme = ScalarTheme.BluePlanet;
    });

    // K. Controller Endpoints
    app.MapControllers();

    // --- 5. RUN THE APPLICATION ---
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