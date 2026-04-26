using ShopManagementSystem.Models;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IHomeRepository
    {
        Task<List<Slider>> GetActiveSlidersAsync();
        Task<List<Category>> GetActiveCategoriesAsync(int take = 8);
        Task<List<Product>> GetLatestProductsAsync(int take = 8);
    }
}
