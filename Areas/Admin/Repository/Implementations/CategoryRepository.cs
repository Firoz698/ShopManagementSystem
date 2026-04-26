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

        // সব category — products সহ, নতুন থেকে পুরনো
        public async Task<List<Category>> GetAllWithProductsAsync()
        {
            return await _db.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Id দিয়ে category খোঁজে
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

        // Category exist করে কিনা check
        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Categories.AnyAsync(c => c.Id == id);
        }
    }
}
