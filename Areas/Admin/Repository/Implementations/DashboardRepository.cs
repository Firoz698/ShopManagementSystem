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

        // মোট product count
        public async Task<int> GetTotalProductsAsync()
            => await _db.Products.CountAsync();

        // মোট order count
        public async Task<int> GetTotalOrdersAsync()
            => await _db.Orders.CountAsync();

        // মোট user count
        public async Task<int> GetTotalUsersAsync()
            => await _db.Users.CountAsync();

        // Pending অর্ডারের count
        public async Task<int> GetPendingOrdersAsync()
            => await _db.Orders.CountAsync(o => o.Status == "Pending");

        // শুধু Completed অর্ডারের মোট revenue
        public async Task<decimal> GetTotalRevenueAsync()
            => await _db.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

        // সর্বশেষ orders — user সহ
        public async Task<List<Order>> GetRecentOrdersAsync(int take = 10)
            => await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .ToListAsync();

        // Stock কম এমন products (threshold এর সমান বা কম)
        public async Task<List<Product>> GetLowStockItemsAsync(int threshold = 5, int take = 10)
            => await _db.Products
                .Where(p => p.Stock <= threshold)
                .Take(take)
                .ToListAsync();
    }
}
