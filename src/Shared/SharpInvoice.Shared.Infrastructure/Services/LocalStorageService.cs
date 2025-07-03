namespace SharpInvoice.Shared.Infrastructure.Services;

using Microsoft.AspNetCore.Hosting;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

public class LocalStorageService(IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string containerName)
    {
        // Sanitize inputs for security
        var safeFileName = SanitizeFileName(fileName);
        var safeContainerName = SanitizeDirectoryName(containerName);
        
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(safeFileName)}";
        var folderPath = Path.Combine(webHostEnvironment.WebRootPath, safeContainerName);
        var filePath = Path.Combine(folderPath, uniqueFileName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        using (var newFileStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(newFileStream);
        }

        return $"/{safeContainerName}/{uniqueFileName}";
    }
    
    /// <summary>
    /// Sanitizes a filename to prevent path traversal and other security issues.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "file";
            
        // Remove any path information to avoid directory traversal
        fileName = Path.GetFileName(fileName);
        
        // Replace any characters that aren't alphanumeric, underscore, dash, or dot
        // This helps prevent command injection and other security issues
        fileName = Regex.Replace(fileName, @"[^\w\-\.]", "_");
        
        // Ensure the filename is not empty after sanitization
        return string.IsNullOrWhiteSpace(fileName) ? "file" : fileName;
    }
    
    /// <summary>
    /// Sanitizes a directory name to prevent path traversal.
    /// </summary>
    private static string SanitizeDirectoryName(string directoryName)
    {
        if (string.IsNullOrWhiteSpace(directoryName))
            return "files";
            
        // Replace any characters that aren't alphanumeric, underscore, or dash
        // This prevents directory traversal and other security issues
        directoryName = Regex.Replace(directoryName, @"[^\w\-]", "_");
        
        // Ensure the directory name is not empty after sanitization
        return string.IsNullOrWhiteSpace(directoryName) ? "files" : directoryName;
    }
} 