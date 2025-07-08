namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;

public record IsBusinessNameAvailableQuery(string Name, string Country) : IRequest<bool>;