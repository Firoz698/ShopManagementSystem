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

 // Active slider SortOrder 
        public async Task<List<Slider>> GetActiveSlidersAsync()
        {
            return await _db.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

 // Active category (homepage )
        public async Task<List<Category>> GetActiveCategoriesAsync(int take = 8)
        {
            return await _db.Categories
                .Where(c => c.IsActive)
                .Take(take)
                .ToListAsync();
        }

 // active product — images, category sizes 
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
