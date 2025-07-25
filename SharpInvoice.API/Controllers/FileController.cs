using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for file management operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class FileController : ApiControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<FileController> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileController"/> class.
    /// </summary>
    /// <param name="storageService">The storage service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    public FileController(
        IStorageService storageService,
        ILogger<FileController> logger,
        IConfiguration configuration)
    {
        _storageService = storageService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <remarks>
    /// Uploads a file to the configured storage provider (local or Azure Blob Storage).
    /// Supports various file types with size and format validation.
    /// 
    /// Supported file types:
    /// - Images: PNG, JPG, JPEG, GIF, WEBP (max 10MB)
    /// - Documents: PDF, DOC, DOCX, XLS, XLSX (max 25MB)
    /// - Archives: ZIP, RAR (max 50MB)
    /// 
    /// Sample request (multipart/form-data):
    /// 
    ///     POST /api/v1/file/upload
    ///     Content-Type: multipart/form-data
    ///     Authorization: Bearer {your-access-token}
    ///     
    ///     file: [file data]
    ///     category: "logo" | "document" | "invoice_attachment"
    ///     description: "Optional file description"
    /// 
    /// </remarks>
    /// <param name="file">The file to upload</param>
    /// <param name="category">File category for organization</param>
    /// <param name="description">Optional description of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with file URL and metadata</returns>
    /// <response code="200">File uploaded successfully.</response>
    /// <response code="400">Invalid file, missing data, or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="413">File size exceeds the maximum allowed limit.</response>
    /// <response code="415">Unsupported file type.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFileAsync(
        [FromForm, Required] IFormFile file,
        [FromForm] string category = "general",
        [FromForm] string? description = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Invalid("File is required and cannot be empty.");
        }

        _logger.LogInformation("File upload requested: {FileName}, Size: {FileSize}, Category: {Category}", 
            file.FileName, file.Length, category);

        try
        {
            // Validate file type and size
            var validationResult = ValidateFile(file, category);
            if (!validationResult.IsValid)
            {
                return validationResult.Result;
            }

            // Generate unique file name
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{CurrentUserId}_{Guid.NewGuid()}{fileExtension}";
            var containerPath = $"{category}/{DateTime.UtcNow:yyyy/MM}";

            // Upload to storage
            using var stream = file.OpenReadStream();
            var fileUrl = await _storageService.UploadFileAsync(containerPath, uniqueFileName, stream);

            var response = new FileUploadResponse
            {
                FileId = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                UniqueFileName = uniqueFileName,
                FileUrl = fileUrl,
                ContentType = file.ContentType,
                Size = file.Length,
                Category = category,
                Description = description,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = CurrentUserId!
            };

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FileUrl}", file.FileName, fileUrl);

            return Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            throw;
        }
    }

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <remarks>
    /// Downloads a file from storage using its unique file path.
    /// The user must have access to the file (own it or have permissions).
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/file/download?filePath=logo/2024/01/user123_abc123.png
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="filePath">The storage path of the file to download</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File download stream</returns>
    /// <response code="200">File downloaded successfully.</response>
    /// <response code="400">Invalid file path.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied to the file.</response>
    /// <response code="404">File not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFileAsync(
        [FromQuery, Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Invalid("File path is required.");
        }

        _logger.LogInformation("File download requested: {FilePath} by user: {UserId}", filePath, CurrentUserId);

        try
        {
            // Validate user has access to this file
            if (!HasFileAccess(filePath))
            {
                return Forbidden("Access denied to this file.");
            }

            // Get file from storage
            var fileStream = await _storageService.GetFileAsync("files", filePath);
            if (fileStream == null)
            {
                return NotFound("File not found.");
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);

            _logger.LogInformation("File download successful: {FilePath}", filePath);

            return File(fileStream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <remarks>
    /// Permanently deletes a file from storage. This action cannot be undone.
    /// The user must own the file or have deletion permissions.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/file?filePath=logo/2024/01/user123_abc123.png
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="filePath">The storage path of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="204">File deleted successfully.</response>
    /// <response code="400">Invalid file path.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied to delete this file.</response>
    /// <response code="404">File not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFileAsync(
        [FromQuery, Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Invalid("File path is required.");
        }

        _logger.LogWarning("File deletion requested: {FilePath} by user: {UserId}", filePath, CurrentUserId);

        try
        {
            // Validate user has access to delete this file
            if (!HasFileAccess(filePath))
            {
                return Forbidden("Access denied to delete this file.");
            }

            // Delete from storage
            await _storageService.DeleteFileAsync("files", filePath);

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Lists files in a specific category
    /// </summary>
    /// <remarks>
    /// Retrieves a list of files in a specific category that the user has access to.
    /// Supports pagination and filtering options.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/file/list?category=logo&page=1&pageSize=20
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="category">File category to filter by</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of files</returns>
    /// <response code="200">Files retrieved successfully.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(FileListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListFilesAsync(
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Invalid("Page number must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Invalid("Page size must be between 1 and 100.");
        }

        _logger.LogDebug("File list requested: Category: {Category}, Page: {Page}, PageSize: {PageSize}", 
            category, page, pageSize);

        try
        {
            // In a real implementation, this would query a database of file metadata
            // For now, returning mock data
            var files = GenerateMockFileList(category, CurrentUserId!);
            
            var totalCount = files.Count;
            var pagedFiles = files
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new FileListResponse
            {
                Files = pagedFiles,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files for category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// Gets file information without downloading
    /// </summary>
    /// <remarks>
    /// Retrieves metadata information about a file without downloading its content.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/file/info?filePath=logo/2024/01/user123_abc123.png
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="filePath">The storage path of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File metadata information</returns>
    /// <response code="200">File information retrieved successfully.</response>
    /// <response code="400">Invalid file path.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied to the file.</response>
    /// <response code="404">File not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(FileInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileInfoAsync(
        [FromQuery, Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Invalid("File path is required.");
        }

        _logger.LogDebug("File info requested: {FilePath}", filePath);

        try
        {
            // Validate user has access to this file
            if (!HasFileAccess(filePath))
            {
                return Forbidden("Access denied to this file.");
            }

            // In a real implementation, this would query file metadata from database
            var fileInfo = new FileInfoResponse
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                ContentType = GetContentType(filePath),
                Size = 1024000, // Mock size
                Category = ExtractCategoryFromPath(filePath),
                UploadedAt = DateTime.UtcNow.AddDays(-30), // Mock date
                UploadedBy = CurrentUserId!,
                LastModified = DateTime.UtcNow.AddDays(-5),
                IsPubliclyAccessible = false
            };

            return Success(fileInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
            throw;
        }
    }

    #region Private Helper Methods

    private (bool IsValid, IActionResult Result) ValidateFile(IFormFile file, string category)
    {
        var maxSizeBytes = GetMaxFileSizeForCategory(category);
        var allowedTypes = GetAllowedFileTypesForCategory(category);

        // Check file size
        if (file.Length > maxSizeBytes)
        {
            var maxSizeMB = maxSizeBytes / (1024 * 1024);
            return (false, StatusCode(413, new ProblemDetails
            {
                Status = 413,
                Title = "File Too Large",
                Detail = $"File size exceeds the maximum allowed limit of {maxSizeMB}MB for category '{category}'."
            }));
        }

        // Check file type
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedTypes.Contains(fileExtension))
        {
            return (false, StatusCode(415, new ProblemDetails
            {
                Status = 415,
                Title = "Unsupported File Type",
                Detail = $"File type '{fileExtension}' is not supported for category '{category}'. " +
                        $"Allowed types: {string.Join(", ", allowedTypes)}"
            }));
        }

        return (true, null!);
    }

    private static long GetMaxFileSizeForCategory(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "logo" or "image" => 10 * 1024 * 1024, // 10MB
            "document" => 25 * 1024 * 1024, // 25MB
            "archive" => 50 * 1024 * 1024, // 50MB
            _ => 10 * 1024 * 1024 // Default 10MB
        };
    }

    private static string[] GetAllowedFileTypesForCategory(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "logo" or "image" => new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" },
            "document" => new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" },
            "archive" => new[] { ".zip", ".rar", ".7z" },
            _ => new[] { ".png", ".jpg", ".jpeg", ".pdf", ".doc", ".docx" }
        };
    }

    private bool HasFileAccess(string filePath)
    {
        // Simple access control - user can access files they uploaded
        // In a real system, this would be more sophisticated
        return filePath.Contains(CurrentUserId!);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".zip" => "application/zip",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static string ExtractCategoryFromPath(string filePath)
    {
        var parts = filePath.Split('/');
        return parts.Length > 0 ? parts[0] : "general";
    }

    private static List<FileMetadata> GenerateMockFileList(string? category, string userId)
    {
        var files = new List<FileMetadata>
        {
            new() { FilePath = $"logo/2024/01/{userId}_logo1.png", FileName = "company-logo.png", Size = 256000, UploadedAt = DateTime.UtcNow.AddDays(-10) },
            new() { FilePath = $"document/2024/01/{userId}_invoice.pdf", FileName = "invoice-template.pdf", Size = 512000, UploadedAt = DateTime.UtcNow.AddDays(-5) },
            new() { FilePath = $"image/2024/01/{userId}_banner.jpg", FileName = "banner-image.jpg", Size = 1024000, UploadedAt = DateTime.UtcNow.AddDays(-3) }
        };

        if (!string.IsNullOrEmpty(category))
        {
            files = files.Where(f => f.FilePath.StartsWith(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return files;
    }

    #endregion
}

#region DTOs

/// <summary>
/// File upload response data transfer object
/// </summary>
public sealed record FileUploadResponse
{
    public string FileId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string UniqueFileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime UploadedAt { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
}

/// <summary>
/// File list response data transfer object
/// </summary>
public sealed record FileListResponse
{
    public List<FileMetadata> Files { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// File metadata data transfer object
/// </summary>
public sealed record FileMetadata
{
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// File information response data transfer object
/// </summary>
public sealed record FileInfoResponse
{
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
    public bool IsPubliclyAccessible { get; init; }
}

#endregion