
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using SharpInvoice.API.Middleware;
using SharpInvoice.API.Middleware.Exceptions;
using SharpInvoice.API.Validators;
using System.Text;

namespace SharpInvoice.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure a logger for application startup
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting SharpInvoice Web API host...");

                var builder = WebApplication.CreateBuilder(args);

                // Add Azure Key Vault configuration (only if using Azure)
                // Uncomment the following lines if deploying to Azure and using Key Vault
                /*
                var keyVaultUrl = builder.Configuration["KeyVault:Url"];
                if (!string.IsNullOrEmpty(keyVaultUrl))
                {
                    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
                }
                */

                // Configure Serilog
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                // Add Infrastructure Services (Database, Repositories, Services)
                builder.Services.AddInfrastructureServices(builder.Configuration);

                // Add API Services (Application Services, Validation, AutoMapper, Exception Handling)
                builder.Services.AddApiServices(builder.Configuration);

                // Configure FluentValidation
                builder.Services.AddValidatorsFromAssemblyContaining<CreateBusinessRequestValidator>();

                // Configure AppSettings
                builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
                builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

                // Add caching services
                builder.Services.AddMemoryCache();
                builder.Services.AddDistributedMemoryCache();

                // Add Microsoft.AspNetCore.OpenApi to generate the OpenAPI metadata.
                builder.Services.AddOpenApi(options =>
                {
                    options.AddDocumentTransformer(async (document, context, _) =>
                    {
                        document.Info = new OpenApiInfo
                        {
                            Title = "SharpInvoice API",
                            Version = "v1.0.0",
                            Description = @"
## SharpInvoice Professional API

A comprehensive invoicing and business management API designed for freelancers, small businesses, and service providers.

### Features
- **User Management**: Registration, authentication, and profile management
- **Business Profiles**: Multi-business support with branding and settings  
- **Invoice Management**: Create, send, and track invoices with automated numbering
- **Client Management**: Organize customer information and billing details
- **Payment Integration**: Stripe integration for online payments
- **Reporting**: Business analytics and financial insights

### Authentication
This API uses JWT Bearer token authentication. Include your access token in the Authorization header:
```
Authorization: Bearer your-access-token-here
```

### Getting Started
1. Register a new account using `/api/v1/auth/register`
2. Login to get your access token using `/api/v1/auth/login`
3. Use the token to access protected endpoints
4. Create your business profile and start managing invoices

### Rate Limiting  
API requests are limited to 100 requests per minute per user.

### Support
For API support and questions, contact our team at support@sharpinvoice.com
            ",
                            TermsOfService = new Uri("https://sharpinvoice.com/terms"),
                            Contact = new OpenApiContact
                            {
                                Name = "SharpInvoice API Support",
                                Email = "support@sharpinvoice.com",
                                Url = new Uri("https://sharpinvoice.com/support")
                            },
                            License = new OpenApiLicense
                            {
                                Name = "MIT License",
                                Url = new Uri("https://opensource.org/licenses/MIT")
                            }
                        };
                        await Task.CompletedTask;
                    });
                    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                    options.AddDocumentTransformer<OpenApiDocumentTransformer>();
                });

                var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
                    ?? throw new InvalidOperationException("AppSettings is missing or invalid. Check your configuration.");

                // Configure Authentication & Authorization
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Secret)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                })
                .AddGoogle(options =>
                {
                    options.ClientId = appSettings.Authentication.Google.ClientId;
                    options.ClientSecret = appSettings.Authentication.Google.ClientSecret;
                });

                // Add Authorization
                builder.Services.AddAuthorizationBuilder()
                    .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
                    .AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));

                // Add CORS policy
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
                    {
                        policyBuilder
                            .WithOrigins(appSettings.AllowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                });

                // Add Rate Limiting
                builder.Services.AddRateLimiter(options =>
                {
                    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
                        httpContext => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 100,
                                Window = TimeSpan.FromMinutes(1)
                            }));
                });


                // Add Health Checks
                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<SharpInvoice.Infrastructure.Persistence.AppDbContext>();

                // Add Problem Details
                builder.Services.AddProblemDetails();

                // Add Exception Handler
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

                var app = builder.Build();

                // Map the OpenAPI endpoint(for the raw JSON document) and cache the output.
                app.MapOpenApi("/api/{documentName}/openapi.json").CacheOutput();

                // Map the Scalar API Reference UI with full customization.
                app.MapScalarApiReference("/scalar/v1", options =>
                {
                    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
                    options.Theme = ScalarTheme.DeepSpace;
                    options.TagSorter = TagSorter.Alpha;
                    options.OperationSorter = OperationSorter.Method;
                    options.OpenApiRoutePattern = "api/{documentName}/openapi.json";
                });

                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }

                // Use exception handling
                app.UseExceptionHandler();

                // Use static files
                app.UseStaticFiles();

                // Use HTTPS redirection
                app.UseHttpsRedirection();

                // Use CORS
                app.UseCors("AllowSpecificOrigin");

                // Use Rate Limiting
                app.UseRateLimiter();

                // Use Authentication & Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                // Map Controllers
                app.MapControllers();

                // Map Health Checks
                app.MapHealthChecks("/health");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }


    // Transformer to add the Bearer security scheme to the OpenAPI document.
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            // Check if a Bearer authentication scheme is configured.
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                // Define the Bearer security scheme.
                var bearerScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

                // Ensure the Components and SecuritySchemes dictionaries are initialized.
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

                // Set the Bearer scheme with details.
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                      "Enter 'Bearer' [space] and then your token.\r\n\r\n" +
                                      "Example: \"Bearer 1safsfsdfdfd\""
                };

                // Iterate over all operations.
                foreach (var operation in document.Paths.SelectMany(path => path.Value.Operations))
                {
                    // Only add a security requirement if the operation already has some security metadata.
                    // This typically indicates that the endpoint has the [Authorize] attribute.
                    if (operation.Value.Security != null && operation.Value.Security.Any())
                    {
                        operation.Value.Security.Add(new OpenApiSecurityRequirement
                        {
                            [bearerScheme] = Array.Empty<string>()
                        });
                    }
                }
            }
        }
    }
}
