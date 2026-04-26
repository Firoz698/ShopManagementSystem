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

        // শুধু Completed অর্ডার খোঁজে (review এর জন্য)
        public async Task<Order?> GetCompletedOrderAsync(int orderId, string userId)
        {
            return await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == "Completed");
        }

        // User আগে এই product এ review দিয়েছে কিনা
        public async Task<bool> HasUserReviewedProductAsync(string userId, int productId)
        {
            return await _db.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        // নতুন review save
        public async Task AddReviewAsync(string userId, int productId, int rating, string? comment)
        {
            _db.Reviews.Add(new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment
            });
            await _db.SaveChangesAsync();
        }

        // Order details সহ order খোঁজে (return request এর জন্য)
        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        // User আগে এই product এর জন্য return request দিয়েছে কিনা
        public async Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId)
        {
            return await _db.ReturnRequests
                .AnyAsync(r => r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);
        }

        // নতুন return request save
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

        // User এর সব return request — product ও order সহ
        public async Task<List<ReturnRequest>> GetMyReturnsAsync(string userId)
        {
            return await _db.ReturnRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
