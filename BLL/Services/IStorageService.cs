namespace BLL.Services;

public interface IStorageService
{
    /// <summary>
    /// Upload an image file to Cloudflare R2 storage.
    /// </summary>
    /// <param name="file">The file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type (e.g. image/jpeg, image/png)</param>
    /// <param name="folder">Optional folder prefix (e.g. "products", "reviews")</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadImageAsync(Stream file, string fileName, string contentType, string folder = "images");

    /// <summary>
    /// Upload a 3D model file (.glb, .gltf) to Cloudflare R2 storage.
    /// </summary>
    /// <param name="file">The file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type (e.g. model/gltf-binary)</param>
    /// <param name="folder">Optional folder prefix (e.g. "models")</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> Upload3DFileAsync(Stream file, string fileName, string contentType, string folder = "models");

    /// <summary>
    /// Delete a file from storage by its URL.
    /// </summary>
    Task DeleteByUrlAsync(string url);
}
