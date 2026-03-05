using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<StorageController> _logger;

    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private static readonly string[] Allowed3DTypes = { "model/gltf-binary", "model/gltf+json", "application/octet-stream" };

    public StorageController(IStorageService storageService, ILogger<StorageController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            // Simple no-op operation to verify that the storage service is wired correctly.
            // For CloudflareR2, this will validate credentials on first use.
            using var ms = new MemoryStream(new byte[] { 0 });
            await _storageService.UploadImageAsync(ms, "health-check.png", "image/png", "health-check");
            return Ok(new { ok = true, message = "Storage service is reachable and credentials are valid." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed");
            return StatusCode(500, new { ok = false, message = "Storage health check failed", error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    /// <summary>Upload an image to Cloudflare R2. Returns the public URL.</summary>
    [HttpPost("upload/image")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UploadResponse>> UploadImage(IFormFile file, [FromQuery] string? folder = "products")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var contentType = file.ContentType;
        if (!AllowedImageTypes.Contains(contentType))
            return BadRequest(new { message = $"Invalid image type. Allowed: {string.Join(", ", AllowedImageTypes)}" });

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storageService.UploadImageAsync(stream, file.FileName, contentType, folder ?? "products");
            return Ok(new UploadResponse(url));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải ảnh lên" });
        }
    }

    /// <summary>Upload a 3D model file (.glb, .gltf) to Cloudflare R2. Returns the public URL.</summary>
    [HttpPost("upload/3d")]
    [AllowAnonymous] // Dev-only: allow testing 3D upload without auth
    public async Task<ActionResult<UploadResponse>> Upload3DFile(IFormFile file, [FromQuery] string? folder = "models")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".glb" && ext != ".gltf")
            return BadRequest(new { message = "Only .glb and .gltf files are allowed" });

        var contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType))
            contentType = ext == ".glb" ? "model/gltf-binary" : "model/gltf+json";

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storageService.Upload3DFileAsync(stream, file.FileName, contentType, folder ?? "models");
            return Ok(new UploadResponse(url));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading 3D file");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải file 3D lên" });
        }
    }

    public record UploadResponse(string Url);
}
