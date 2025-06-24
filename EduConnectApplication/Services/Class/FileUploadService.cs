using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduConnect.Infrastruture.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EduConnectApplication.Services.Class
{
    public class FileUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public FileUploadService(Cloudinary cloudinary, IOptions<CloudinarySettings> settings)
        {
            _cloudinary = cloudinary;
            _settings = settings.Value;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UploadPreset = _settings.UploadPreset
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }
    }
} 