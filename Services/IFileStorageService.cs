using Microsoft.AspNetCore.Http;

namespace ZE.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Validates and stores an uploaded image under wwwroot/uploads/{subFolder}.
    /// Returns the web path (e.g. /uploads/projects/abc.png) or null when invalid.
    /// </summary>
    Task<string?> SaveImageAsync(IFormFile? file, string subFolder);

    /// <summary>Deletes a previously stored image given its web path. Safe against paths outside wwwroot/uploads.</summary>
    void DeleteImage(string? webPath);
}
