using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Implementations
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _db;

        public WishlistRepository(ApplicationDbContext db)
        {
            _db = db;
        }

 // User wishlist item — product, image category 
        public async Task<List<Wishlist>> GetWishlistItemsAsync(string userId)
        {
            return await _db.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Product).ThenInclude(p => p!.Images)
                .Include(w => w.Product).ThenInclude(p => p!.Category)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

 // ProductId wishlist item (Toggle )
        public async Task<Wishlist?> GetByProductIdAsync(string userId, int productId)
        {
            return await _db.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

 // Wishlist Id item (Remove )
        public async Task<Wishlist?> GetByIdAsync(int id, string userId)
        {
            return await _db.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
        }

 // wishlist item 
        public async Task AddAsync(string userId, int productId)
        {
            _db.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
            await _db.SaveChangesAsync();
        }

 // Wishlist item 
        public async Task RemoveAsync(Wishlist item)
        {
            _db.Wishlists.Remove(item);
            await _db.SaveChangesAsync();
        }
    }
}
