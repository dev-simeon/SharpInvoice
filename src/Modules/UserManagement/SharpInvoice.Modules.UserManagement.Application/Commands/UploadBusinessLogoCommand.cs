namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using System.IO;

public record UploadBusinessLogoCommand(Guid BusinessId, Stream LogoStream, string FileName) : IRequest;