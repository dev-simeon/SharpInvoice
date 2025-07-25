using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Entities;
using SharpInvoice.API.Interfaces.Services;

namespace SharpInvoice.API.Services;

public class ClientService(IUnitOfWork unitOfWork) : IClientService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest createClientRequest)
    {
        var client = new Client(
            createClientRequest.BusinessId.ToString(),
            createClientRequest.Name,
            createClientRequest.Email,
            createClientRequest.Phone,
            createClientRequest.Address,
            createClientRequest.Country
        );

        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return ToClientDto(client);
    }

    public async Task DeleteClientAsync(string clientId)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client != null)
        {
            await _unitOfWork.Clients.RemoveAsync(client);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<ClientDto?> GetClientByIdAsync(string clientId)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        return client == null ? null : ToClientDto(client);
    }

    public async Task<IEnumerable<ClientDto>> GetClientsByBusinessAsync(string businessId)
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.BusinessId == businessId.ToString());
        return clients.Select(ToClientDto);
    }

    public async Task<ClientDto> UpdateClientAsync(string clientId, CreateClientRequest updateClientRequest)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new Exception("Client not found.");
        }

        client.UpdateName(updateClientRequest.Name);
        client.UpdateContactInfo(updateClientRequest.Email, updateClientRequest.Phone);
        client.UpdateAddress(updateClientRequest.Address, updateClientRequest.Country, null);

        await _unitOfWork.Clients.UpdateAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return ToClientDto(client);
    }
    
    private ClientDto ToClientDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            BusinessId = Guid.Parse(client.BusinessId),
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            Country = client.Country
        };
    }
} 