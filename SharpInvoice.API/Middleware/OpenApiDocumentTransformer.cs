using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SharpInvoice.API.Middleware;

/// <summary>
/// Modern OpenAPI document transformer for .NET 9 that enhances documentation with validation attributes and examples.
/// </summary>
public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Enhance all schemas with validation attributes and examples
        if (document.Components?.Schemas != null)
        {
            foreach (var schema in document.Components.Schemas)
            {
                ApplySchemaEnhancements(schema.Value, schema.Key);
            }
        }

        // Enhance all operations with better descriptions and examples
        foreach (var pathItem in document.Paths.Values)
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                ApplyOperationEnhancements(operation);
            }
        }

        return Task.CompletedTask;
    }

    private static void ApplySchemaEnhancements(OpenApiSchema schema, string schemaName)
    {
        // Add examples for common DTOs
        if (schemaName.Contains("Request") || schemaName.Contains("Dto"))
        {
            AddSchemaExamples(schema, schemaName);
        }

        // Enhance property descriptions and validation
        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                ApplyPropertyEnhancements(property.Value, property.Key);
            }
        }
    }

    private static void AddSchemaExamples(OpenApiSchema schema, string schemaName)
    {
        var example = schemaName switch
        {
            var name when name.Contains("LoginRequest") => new
            {
                email = "user@example.com",
                password = "SecurePass123!"
            },
            var name when name.Contains("RegisterRequest") => new
            {
                email = "user@example.com",
                firstName = "John",
                lastName = "Doe", 
                password = "SecurePass123!",
                phoneNumber = "+1234567890"
            },
            var name when name.Contains("CreateBusinessRequest") => new
            {
                name = "Acme Consulting LLC",
                email = "contact@acmeconsulting.com",
                phone = "+1-555-0123",
                website = "https://acmeconsulting.com",
                address = "123 Business Ave",
                city = "New York",
                state = "NY",
                country = "United States",
                zipCode = "10001"
            },
            var name when name.Contains("CreateClientRequest") => new
            {
                businessId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                name = "Acme Corporation",
                email = "billing@acme.com",
                phone = "+1-555-0123",
                address = "123 Business Street, New York, NY 10001",
                country = "United States"
            },
            var name when name.Contains("CreateInvoiceRequest") => new
            {
                businessId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                clientId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                issueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                dueDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                currency = "USD",
                notes = "Thank you for your business!",
                terms = "Payment due within 30 days",
                items = new[]
                {
                    new
                    {
                        description = "Consulting Services",
                        quantity = 10.0,
                        unitPrice = 150.00,
                        unit = "hours"
                    }
                }
            },
            _ => null
        };

        if (example != null)
        {
            schema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson(
                System.Text.Json.JsonSerializer.Serialize(example));
        }
    }

    private static void ApplyPropertyEnhancements(OpenApiSchema propertySchema, string propertyName)
    {
        // Add format and pattern for common property types
        switch (propertyName.ToLowerInvariant())
        {
            case "email":
                propertySchema.Format = "email";
                propertySchema.Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"user@example.com\"");
                break;
            case "phonenumber" or "phone":
                propertySchema.Format = "phone";
                propertySchema.Pattern = @"^\+?[1-9]\d{1,14}$";
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"+1234567890\"");
                break;
            case "website" or "url":
                propertySchema.Format = "uri";
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"https://example.com\"");
                break;
            case "password":
                propertySchema.Format = "password";
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"SecurePass123!\"");
                break;
            case var name when name.EndsWith("id"):
                propertySchema.Format = "uuid";
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"");
                break;
            case "currency":
                propertySchema.Example = Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"USD\"");
                break;
        }

        // Add descriptions for common properties
        if (string.IsNullOrEmpty(propertySchema.Description))
        {
            propertySchema.Description = propertyName.ToLowerInvariant() switch
            {
                "email" => "Email address in valid format",
                "password" => "Password with minimum 8 characters including uppercase, lowercase, digit, and special character",
                "firstname" => "User's first name",
                "lastname" => "User's last name", 
                "phonenumber" or "phone" => "Phone number in international format",
                "website" or "url" => "Valid website URL",
                "businessid" => "Unique identifier of the business",
                "clientid" => "Unique identifier of the client",
                "invoiceid" => "Unique identifier of the invoice",
                "userid" => "Unique identifier of the user",
                "currency" => "Three-letter currency code (e.g., USD, EUR, GBP)",
                "issuedate" => "Date when the invoice was issued",
                "duedate" => "Date when payment is due",
                "status" => "Current status of the item",
                _ => null
            };
        }
    }

    private static void ApplyOperationEnhancements(OpenApiOperation operation)
    {
        // Enhance parameter descriptions
        if (operation.Parameters != null)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (string.IsNullOrEmpty(parameter.Description))
                {
                    parameter.Description = parameter.Name?.ToLowerInvariant() switch
                    {
                        "page" => "Page number for pagination (default: 1)",
                        "pagesize" => "Number of items per page (default: 10, max: 100)",
                        "sortby" => "Field to sort by",
                        "sortorder" => "Sort order (asc or desc)",
                        "status" => "Filter by status",
                        "period" => "Time period for statistics (month, quarter, year)",
                        "searchterm" => "Search term for filtering results",
                        var name when name.EndsWith("id") => $"Unique identifier for the {name.Replace("id", "")}",
                        _ => $"The {parameter.Name} parameter"
                    };
                }

                // Add examples for query parameters
                if (parameter.In == ParameterLocation.Query && parameter.Example == null)
                {
                    parameter.Example = parameter.Name?.ToLowerInvariant() switch
                    {
                        "page" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("1"),
                        "pagesize" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("10"),
                        "sortby" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"issueDate\""),
                        "sortorder" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"desc\""),
                        "status" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"draft\""),
                        "period" => Microsoft.OpenApi.Any.OpenApiAnyFactory.CreateFromJson("\"month\""),
                        _ => null
                    };
                }
            }
        }

        // Ensure all responses have proper media types
        foreach (var response in operation.Responses.Values)
        {
            if (response.Content?.Count == 0)
            {
                response.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema { Type = "object" }
                };
            }
        }
    }
} 