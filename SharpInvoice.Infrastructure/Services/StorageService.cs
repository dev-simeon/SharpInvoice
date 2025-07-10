using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Shared;

namespace SharpInvoice.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public StorageService(IOptions<AppSettings> appSettings)
    {
        var storageSettings = appSettings.Value.AzureStorage;
        _containerName = storageSettings.ContainerName;
        
        _blobServiceClient = new BlobServiceClient(storageSettings.ConnectionString);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        
        // Create a unique file name to prevent overwrites
        string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        
        var blobClient = containerClient.GetBlobClient(uniqueFileName);
        
        // Set the content type
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };
        
        await blobClient.UploadAsync(fileStream, options);
        
        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return false;
            
        try
        {
            Uri uri = new(fileUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            return await blobClient.DeleteIfExistsAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> GetFileAsync(string fileUrl)
    {
        Uri uri = new(fileUrl);
        string blobName = Path.GetFileName(uri.LocalPath);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public string GetSignedUrl(string fileUrl, TimeSpan expiry)
    {
        Uri uri = new(fileUrl);
        string blobName = Path.GetFileName(uri.LocalPath);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        
        // Create a SAS token that's valid for the specified duration
        BlobSasBuilder sasBuilder = new()
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b", // b for blob
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        
        // Generate the SAS token
        var sasToken = blobClient.GenerateSasUri(sasBuilder);
        
        return sasToken.ToString();
    }
} 