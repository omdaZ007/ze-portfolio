using Microsoft.AspNetCore.Hosting;

namespace ZE.Services;

/// <summary>
/// Stores uploaded images on disk under wwwroot/uploads with random, safe filenames.
/// Validates extension, MIME type and size; never trusts the client-provided filename.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/png", "image/webp" };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env) => _env = env;

    public Task<string?> SaveImageAsync(IFormFile? file, string subFolder)
    {
        if (file is null || file.Length == 0) return Task.FromResult<string?>(null);
        if (file.Length > MaxFileSize) return Task.FromResult<string?>(null);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return Task.FromResult<string?>(null);

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Task.FromResult<string?>(null);

        // Re-validate by sniffing the magic bytes so an executable renamed to .png never lands on disk.
        if (!HasValidImageSignature(file))
            return Task.FromResult<string?>(null);

        var folder = SanitizeSubFolder(subFolder);
        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var targetDir = Path.GetFullPath(Path.Combine(uploadsRoot, folder));

        if (!targetDir.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>(null);

        Directory.CreateDirectory(targetDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDir, fileName);

        using var stream = new FileStream(fullPath, FileMode.CreateNew);
        file.CopyTo(stream);

        return Task.FromResult<string?>(Path.Combine("/uploads", folder, fileName).Replace('\\', '/'));
    }

    public void DeleteImage(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath)) return;
        if (!webPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;

        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var relative = webPath["/uploads/".Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relative));

        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch (IOException)
        {
            // Image cleanup must never break the main operation.
        }
    }

    private static string SanitizeSubFolder(string subFolder)
    {
        var cleaned = RegexReplaceInvalid(subFolder);
        return string.IsNullOrWhiteSpace(cleaned) ? "misc" : cleaned;
    }

    private static string RegexReplaceInvalid(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Where(c => !invalid.Contains(c) && c != '/' && c != '\\').ToArray();
        var result = new string(chars).Trim().Trim('.').ToLowerInvariant();
        if (result.Contains("..")) return "misc";
        return result;
    }

    private static bool HasValidImageSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        if (read < 4) return false;

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // WEBP: RIFF....WEBP
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

        return false;
    }
}
