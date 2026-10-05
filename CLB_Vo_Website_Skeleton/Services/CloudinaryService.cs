using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CLBVo.Models;
using Microsoft.Extensions.Options;

namespace CLBVo.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string?> UploadAsync(IFormFile? file, string folder = "clb-vo")
    {
        if (file == null || file.Length == 0) return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp).");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("File không được vượt quá 5MB.");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new InvalidOperationException($"Upload lên Cloudinary thất bại: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteAsync(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        try
        {
            var uri = new Uri(imageUrl);
            var pathParts = uri.AbsolutePath.Split('/');
            var uploadIndex = Array.IndexOf(pathParts, "upload");
            if (uploadIndex == -1) return;

            var publicIdWithVersion = string.Join("/", pathParts.Skip(uploadIndex + 2));
            var publicId = Path.ChangeExtension(publicIdWithVersion, null);

            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Không thể xóa ảnh trên Cloudinary: {ex.Message}");
        }
    }
}