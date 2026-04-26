using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminProductRepository
    {
        // Index
        Task<List<Product>> GetFilteredProductsAsync(string? search, int? categoryId, bool? isActive);
        Task<List<Category>> GetActiveCategoriesAsync();
        Task<List<Category>> GetAllCategoriesAsync();

        // Create
        Task<Product> CreateProductAsync(ProductCreateViewModel vm);
        Task AddProductImagesAsync(int productId, IList<IFormFile> images, IImageService imageService);
        Task AddProductSizesAsync(int productId, List<ProductSizeEntry> sizes);

        // Edit
        Task<Product?> GetProductWithImagesAndSizesAsync(int id);
        Task UpdateProductAsync(Product product, ProductCreateViewModel vm);
        Task UpdatePrimaryImageAsync(Product product, int? primaryImageId);
        Task RemoveImagesAsync(Product product, List<int> removeIds, IImageService imageService);
        Task AddNewImagesAsync(int productId, Product product, IList<IFormFile> images, IImageService imageService);
        Task UpdateSizesAsync(int productId, Product product, List<ProductSizeEntry> sizes);

        // Toggle / Delete
        Task<Product?> GetByIdAsync(int id);
        Task ToggleActiveAsync(Product product);
        Task DeleteProductAsync(Product product, IImageService imageService);

        Task SaveAsync();
    }
}
