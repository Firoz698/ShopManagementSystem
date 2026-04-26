using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IProductRepository
    {
        Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
            int? categoryId, string? search,
            decimal? minPrice, decimal? maxPrice,
            string? sortBy, int page, int pageSize);

        Task<List<Category>> GetActiveCategoriesAsync();

        Task<Product?> GetProductDetailAsync(int id);

        Task<bool> IsInWishlistAsync(string userId, int productId);

        Task<List<Product>> GetRelatedProductsAsync(int categoryId, int excludeProductId);

        Task<bool> HasUserReviewedAsync(string userId, int productId);

        Task AddReviewAsync(string userId, ReviewViewModel vm);
    }
}
