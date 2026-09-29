using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _db;

        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

 // product count
        public async Task<int> GetTotalProductsAsync()
            => await _db.Products.CountAsync();

 // order count
        public async Task<int> GetTotalOrdersAsync()
            => await _db.Orders.CountAsync();

 // user count
        public async Task<int> GetTotalUsersAsync()
            => await _db.Users.CountAsync();

 // Pending count
        public async Task<int> GetPendingOrdersAsync()
            => await _db.Orders.CountAsync(o => o.Status == "Pending");

 // Completed revenue
        public async Task<decimal> GetTotalRevenueAsync()
            => await _db.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

 // orders — user 
        public async Task<List<Order>> GetRecentOrdersAsync(int take = 10)
            => await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .ToListAsync();

 // Stock products (threshold )
        public async Task<List<Product>> GetLowStockItemsAsync(int threshold = 5, int take = 10)
            => await _db.Products
                .Where(p => p.Stock <= threshold)
                .Take(take)
                .ToListAsync();
    }
}

