namespace SharpInvoice.Shared.Infrastructure.Interfaces;

using System.IO;
using System.Threading.Tasks;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string containerName);
}