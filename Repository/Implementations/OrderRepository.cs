using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Repository.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Review ───────────────────────────────────────────────────────────────

 // Completed — review 
        public async Task<Order?> GetCompletedOrderAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId
                    && o.UserId == userId
                    && o.Status == "Completed");
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, int productId)
        {
            return await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task AddReviewAsync(string userId, int productId, int rating, string? comment)
        {
            _db.Reviews.Add(new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment
            });
            await _db.SaveChangesAsync();
        }

        // ── Return Request ───────────────────────────────────────────────────────

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId)
        {
            return await _db.ReturnRequests.AnyAsync(r =>
                r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);
        }

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

        public async Task<List<ReturnRequest>> GetMyReturnsAsync(string userId)
        {
            return await _db.ReturnRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // ── Order Cancel ─────────────────────────────────────────────────────────

 // Pending/Processing cancel 
        public async Task<Order?> GetCancellableOrderAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId
                    && o.UserId == userId
                    && (o.Status == "Pending" || o.Status == "Processing"));
        }

 // Order cancel + stock 
        public async Task CancelOrderAsync(Order order, string? reason)
        {
            foreach (var detail in order.OrderDetails)
            {
                var product = await _db.Products.FindAsync(detail.ProductId);
                if (product != null)
                    product.Stock += detail.Quantity;
            }

            order.Status = "Cancelled";
            order.CancelReason = reason;
            order.CancelledAt = DateTime.Now;

            await _db.SaveChangesAsync();
        }

        // ── Order Tracking ───────────────────────────────────────────────────────

        public async Task<Order?> TrackOrderAsync(int orderId, string phone)
        {
            var cleanPhone = phone.Trim().Replace(" ", "").Replace("-", "");
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedEmployee)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p!.Images)
                .Include(o => o.TrackingLogs.OrderBy(t => t.Timestamp))
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Phone.Replace(" ", "").Replace("-", "").EndsWith(cleanPhone));
        }

        public async Task<Order?> TrackOrderByTrackingNumberAsync(string trackingNumber)
        {
            var tn = trackingNumber.Trim();
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedEmployee)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p!.Images)
                .Include(o => o.TrackingLogs.OrderBy(t => t.Timestamp))
                .FirstOrDefaultAsync(o => o.TrackingNumber == tn);
        }

        public async Task<Order?> TrackOrderByIdAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedEmployee)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p!.Images)
                .Include(o => o.TrackingLogs.OrderBy(t => t.Timestamp))
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetUserRecentOrdersAsync(string userId, int count = 5)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
