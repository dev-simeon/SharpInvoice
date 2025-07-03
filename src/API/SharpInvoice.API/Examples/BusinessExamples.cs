using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System.Collections.Generic;

namespace SharpInvoice.API.Examples
{
    /// <summary>
    /// Provides example data for CreateBusinessRequest in Swagger documentation.
    /// </summary>
    public class CreateBusinessRequestExample : IExamplesProvider<CreateBusinessRequest>
    {
        /// <summary>
        /// Gets example instance of CreateBusinessRequest.
        /// </summary>
        /// <returns>Sample CreateBusinessRequest data.</returns>
        public CreateBusinessRequest GetExamples()
        {
            return new CreateBusinessRequest(
                Name: "Acme Corporation",
                Email: "contact@acmecorp.com",
                Country: "US"
            );
        }
    }

    /// <summary>
    /// Provides example data for BusinessDto in Swagger documentation.
    /// </summary>
    public class BusinessDtoExample : IExamplesProvider<BusinessDto>
    {
        /// <summary>
        /// Gets example instance of BusinessDto.
        /// </summary>
        /// <returns>Sample BusinessDto data.</returns>
        public BusinessDto GetExamples()
        {
            return new BusinessDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name: "Acme Corporation"
            );
        }
    }

    /// <summary>
    /// Provides example data for BusinessDetailsDto in Swagger documentation.
    /// </summary>
    public class BusinessDetailsDtoExample : IExamplesProvider<BusinessDetailsDto>
    {
        /// <summary>
        /// Gets example instance of BusinessDetailsDto.
        /// </summary>
        /// <returns>Sample BusinessDetailsDto data.</returns>
        public BusinessDetailsDto GetExamples()
        {
            return new BusinessDetailsDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name: "Acme Corporation",
                Email: "contact@acmecorp.com",
                PhoneNumber: "+1 (555) 123-4567",
                Website: "https://www.acmecorp.com",
                Address: "123 Main Street",
                City: "San Francisco",
                State: "CA",
                ZipCode: "94105",
                Country: "US",
                LogoUrl: "/logos/acmecorp-logo.png",
                IsActive: true
            );
        }
    }

    /// <summary>
    /// Provides example data for UpdateBusinessDetailsDto in Swagger documentation.
    /// </summary>
    public class UpdateBusinessDetailsDtoExample : IExamplesProvider<UpdateBusinessDetailsDto>
    {
        /// <summary>
        /// Gets example instance of UpdateBusinessDetailsDto.
        /// </summary>
        /// <returns>Sample UpdateBusinessDetailsDto data.</returns>
        public UpdateBusinessDetailsDto GetExamples()
        {
            return new UpdateBusinessDetailsDto(
                Name: "Acme Corporation",
                Email: "contact@acmecorp.com",
                PhoneNumber: "+1 (555) 123-4567",
                Website: "https://www.acmecorp.com",
                Address: "123 Main Street",
                City: "San Francisco",
                State: "CA",
                ZipCode: "94105",
                Country: "US",
                LogoUrl: "/logos/acmecorp-logo.png",
                ThemeSettings: new
                {
                    PrimaryColor = "#007BFF",
                    SecondaryColor = "#6C757D",
                    LogoPosition = "left"
                }
            );
        }
    }

    /// <summary>
    /// Provides example data for a list of BusinessDto in Swagger documentation.
    /// </summary>
    public class BusinessListExample : IExamplesProvider<List<BusinessDto>>
    {
        /// <summary>
        /// Gets example instance of a list of BusinessDto.
        /// </summary>
        /// <returns>Sample list of BusinessDto data.</returns>
        public List<BusinessDto> GetExamples()
        {
            return new List<BusinessDto>
            {
                new BusinessDto(
                    Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    Name: "Acme Corporation"
                ),
                new BusinessDto(
                    Id: Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                    Name: "Globex Industries"
                )
            };
        }
    }
}