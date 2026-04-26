using ShopManagementSystem.Generic.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    // Generic IRepository<Category> এর উপরে Category-specific method গুলো
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetAllWithProductsAsync();
        Task<Category?> GetByIdAsync(int id);  // override — EF FindAsync
        Task<bool> ExistsAsync(int id);
    }
}
