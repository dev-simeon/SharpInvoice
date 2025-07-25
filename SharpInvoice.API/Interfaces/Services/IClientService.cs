using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

/// <summary>
/// Defines the contract for client-related operations.
/// </summary>
public interface IClientService
{
    Task<ClientDto> CreateClientAsync(CreateClientRequest createClientRequest);
    Task<ClientDto?> GetClientByIdAsync(string clientId);
    Task<IEnumerable<ClientDto>> GetClientsByBusinessAsync(string businessId);
    Task<ClientDto> UpdateClientAsync(string clientId, CreateClientRequest updateClientRequest);
    Task DeleteClientAsync(string clientId);
} 