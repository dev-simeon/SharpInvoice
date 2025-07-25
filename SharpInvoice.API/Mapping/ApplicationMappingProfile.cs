using AutoMapper;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Entities;

namespace SharpInvoice.API.Mapping;

/// <summary>
/// AutoMapper profile for application layer mappings.
/// </summary>
public sealed class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.ApplicationUserRole.ToString()));

        CreateMap<Business, BusinessDto>();
        
        CreateMap<Client, ClientDto>();
        
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            
        CreateMap<InvoiceItem, InvoiceItemDto>();

        // Add more mappings as needed
    }
}