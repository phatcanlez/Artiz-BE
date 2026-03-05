using Microsoft.Extensions.Configuration;

namespace BLL.Services;

/// <summary>
/// Fallback storage for development when Cloudflare R2 is not configured.
/// Saves files to ./uploads folder and returns relative URLs.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration configuration)
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = configuration["LocalStorage:BaseUrl"] ?? "/uploads";
        Directory.CreateDirectory(_uploadPath);
    }

    public Task<string> UploadImageAsync(Stream file, string fileName, string contentType, string folder = "images")
    {
        return UploadAsync(file, fileName, folder);
    }

    public Task<string> Upload3DFileAsync(Stream file, string fileName, string contentType, string folder = "models")
    {
        return UploadAsync(file, fileName, folder);
    }

    private async Task<string> UploadAsync(Stream file, string fileName, string folder)
    {
        var ext = Path.GetExtension(fileName);
        var newFileName = $"{Guid.NewGuid():N}{ext}";
        var folderPath = Path.Combine(_uploadPath, folder);
        Directory.CreateDirectory(folderPath);
        var fullPath = Path.Combine(folderPath, newFileName);

        await using var fs = File.Create(fullPath);
        await file.CopyToAsync(fs);

        return $"{_baseUrl.TrimEnd('/')}/{folder}/{newFileName}";
    }

    public Task DeleteByUrlAsync(string url)
    {
        try
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var path = uri.IsAbsoluteUri ? uri.AbsolutePath : url.TrimStart('/');
            var fullPath = Path.Combine(_uploadPath, path.Replace("/uploads/", "").Replace("uploads/", ""));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch { /* ignore */ }
        return Task.CompletedTask;
    }
}
