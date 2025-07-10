using System.IO;

namespace SharpInvoice.Core.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<Stream> GetFileAsync(string fileUrl);
    string GetSignedUrl(string fileUrl, TimeSpan expiry);
} 