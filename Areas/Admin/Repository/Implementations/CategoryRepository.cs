using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Generic.Repository.Implementations;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext db) : base(db) { }

 // category — products , 
        public async Task<List<Category>> GetAllWithProductsAsync()
        {
            return await _db.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

 // Id category 
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

 // Category exist check
        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Categories.AnyAsync(c => c.Id == id);
        }
    }
}

