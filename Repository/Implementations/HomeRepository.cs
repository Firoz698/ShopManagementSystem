using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Implementations
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // Active slider গুলো SortOrder অনুযায়ী
        public async Task<List<Slider>> GetActiveSlidersAsync()
        {
            return await _db.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        // Active category গুলো (homepage এ সর্বোচ্চ ৮টি)
        public async Task<List<Category>> GetActiveCategoriesAsync(int take = 8)
        {
            return await _db.Categories
                .Where(c => c.IsActive)
                .Take(take)
                .ToListAsync();
        }

        // সর্বশেষ active product গুলো — images, category ও sizes সহ
        public async Task<List<Product>> GetLatestProductsAsync(int take = 8)
        {
            return await _db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}