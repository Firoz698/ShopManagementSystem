using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // Filter, search, sort ও pagination সহ product list
        public async Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
            int? categoryId, string? search,
            decimal? minPrice, decimal? maxPrice,
            string? sortBy, int page, int pageSize)
        {
            var query = _db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description != null && p.Description.Contains(search));

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (products, total);
        }

        // শুধু active category গুলো
        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _db.Categories.Where(c => c.IsActive).ToListAsync();
        }

        // Product detail — images, sizes, reviews, user সহ
        public async Task<Product?> GetProductDetailAsync(int id)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        // User এর wishlist এ product আছে কিনা
        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            return await _db.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        // Same category র related products (নিজেকে বাদ দিয়ে)
        public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, int excludeProductId)
        {
            return await _db.Products
                .Where(p => p.CategoryId == categoryId && p.Id != excludeProductId && p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .Take(4)
                .ToListAsync();
        }

        // User আগে review দিয়েছে কিনা
        public async Task<bool> HasUserReviewedAsync(string userId, int productId)
        {
            return await _db.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        // নতুন review save করা
        public async Task AddReviewAsync(string userId, ReviewViewModel vm)
        {
            var alreadyReviewed = await HasUserReviewedAsync(userId, vm.ProductId);
            if (alreadyReviewed) return;

            _db.Reviews.Add(new Review
            {
                UserId = userId,
                ProductId = vm.ProductId,
                Rating = vm.Rating,
                Comment = vm.Comment
            });

            await _db.SaveChangesAsync();
        }
    }
}
