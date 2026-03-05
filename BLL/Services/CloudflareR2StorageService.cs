using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace BLL.Services;

public class CloudflareR2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public CloudflareR2StorageService(IConfiguration configuration)
    {
        var config = configuration.GetSection("CloudflareR2");
        var accountId = config["AccountId"] ?? throw new InvalidOperationException("CloudflareR2:AccountId is required");
        var accessKeyId = config["AccessKeyId"] ?? throw new InvalidOperationException("CloudflareR2:AccessKeyId is required");
        var secretAccessKey = config["SecretAccessKey"] ?? throw new InvalidOperationException("CloudflareR2:SecretAccessKey is required");
        _bucketName = config["BucketName"] ?? throw new InvalidOperationException("CloudflareR2:BucketName is required");
        // PublicBaseUrl: custom domain (e.g. https://cdn.yoursite.com) or r2.dev URL when bucket is public
        _publicBaseUrl = (config["PublicBaseUrl"] ?? "").TrimEnd('/');

        var endpoint = $"https://{accountId}.r2.cloudflarestorage.com";
        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            SignatureVersion = "v4"
        });
    }

    public async Task<string> UploadImageAsync(Stream file, string fileName, string contentType, string folder = "images")
    {
        var key = $"{folder}/{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        return await UploadAsync(file, key, contentType);
    }

    public async Task<string> Upload3DFileAsync(Stream file, string fileName, string contentType, string folder = "models")
    {
        var ext = Path.GetExtension(fileName);
        var key = $"{folder}/{Guid.NewGuid():N}{ext}";
        return await UploadAsync(file, key, contentType);
    }

    private async Task<string> UploadAsync(Stream file, string key, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file,
            ContentType = contentType,
            // Cloudflare R2 does not support Streaming SigV4, so we must
            // at least disable payload signing when using this SDK version.
            DisablePayloadSigning = true
        };

        try
        {
            await _s3Client.PutObjectAsync(request);
            // Return public URL: PublicBaseUrl must be configured (custom domain or r2.dev)
            if (string.IsNullOrEmpty(_publicBaseUrl))
                throw new InvalidOperationException("CloudflareR2:PublicBaseUrl is required for public file URLs");
            return $"{_publicBaseUrl}/{key}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file to R2: {ex.Message}", ex);
        }
    }

    public async Task DeleteByUrlAsync(string url)
    {
        try
        {
            var uri = new Uri(url);
            var key = uri.AbsolutePath.TrimStart('/');
            await _s3Client.DeleteObjectAsync(_bucketName, key);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete file from R2: {ex.Message}", ex);
        }
    }
}

