using ShopManagementSystem.Models;

namespace ShopManagementSystem.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<Wishlist>> GetWishlistItemsAsync(string userId);
        Task<Wishlist?> GetByProductIdAsync(string userId, int productId);
        Task<Wishlist?> GetByIdAsync(int id, string userId);
        Task AddAsync(string userId, int productId);
        Task RemoveAsync(Wishlist item);
    }
}