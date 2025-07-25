using System.IO;

namespace SharpInvoice.Core.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(string containerName, string blobName, Stream content);
    Task<Stream> DownloadFileAsync(string containerName, string blobName);
    Task<Stream> GetFileAsync(string containerName, string blobName);
    Task DeleteFileAsync(string containerName, string blobName);
} 