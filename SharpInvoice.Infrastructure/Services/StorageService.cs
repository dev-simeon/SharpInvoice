using Azure.Storage.Blobs;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

public class StorageService(string connectionString) : IStorageService
{
    private readonly string _connectionString = connectionString;

    public async Task<Stream> DownloadFileAsync(string containerName, string blobName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task<string> UploadFileAsync(string containerName, string blobName, Stream content)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, true);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> GetFileAsync(string containerName, string blobName)
    {
        return await DownloadFileAsync(containerName, blobName);
    }

    public async Task DeleteFileAsync(string containerName, string blobName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
} 