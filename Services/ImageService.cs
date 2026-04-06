namespace ShopManagementSystem.Services
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folder = "products");
        void Delete(string imageUrl);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env) => _env = env;

        public async Task<string> UploadAsync(IFormFile file, string folder = "products")
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "uploads", folder);
            Directory.CreateDirectory(uploadsDir);

            var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var path     = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/uploads/{folder}/{fileName}";
        }

        public void Delete(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
