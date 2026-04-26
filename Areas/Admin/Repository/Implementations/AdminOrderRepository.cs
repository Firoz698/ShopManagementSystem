using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminOrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // Status filter সহ সব order — user ও details সহ
        public async Task<List<Order>> GetFilteredOrdersAsync(string? status)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        // Pending order count (badge এর জন্য)
        public async Task<int> GetPendingOrdersCountAsync()
        {
            return await _db.Orders.CountAsync(o => o.Status == "Pending");
        }

        // ReturnRequest GET এর জন্য — order + user সহ
        public async Task<Order?> GetOrderWithUserAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // ReturnRequest GET এর জন্য — product খোঁজে
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _db.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        // এই order+product এ আগে return request দেওয়া হয়েছে কিনা
        public async Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId)
        {
            return await _db.ReturnRequests
                .AnyAsync(r => r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);
        }

        // Order টি এই user এর কিনা verify করে
        public async Task<Order?> GetUserOrderAsync(int orderId, string userId)
        {
            return await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        // নতুন return request save করে
        public async Task AddReturnRequestAsync(int orderId, int productId, string userId, string reason)
        {
            _db.ReturnRequests.Add(new ReturnRequest
            {
                OrderId = orderId,
                ProductId = productId,
                UserId = userId,
                Reason = reason,
                Status = "Pending"
            });
            await _db.SaveChangesAsync();
        }

        // Order detail page এর জন্য — user, orderDetails, product, images সহ
        public async Task<Order?> GetOrderDetailAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Id দিয়ে order খোঁজে (UpdateStatus ও Delete এর জন্য)
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders.FindAsync(id);
        }

        // Order status ও UpdatedAt update করে
        public async Task UpdateStatusAsync(Order order, string status)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        // Order delete করে
        public async Task DeleteAsync(Order order)
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
        }
    }
}
