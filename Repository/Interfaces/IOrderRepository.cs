using ShopManagementSystem.Models;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetCompletedOrderAsync(int orderId, string userId);
        Task<bool> HasUserReviewedProductAsync(string userId, int productId);
        Task AddReviewAsync(string userId, int productId, int rating, string? comment);

        Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId);
        Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId);
        Task AddReturnRequestAsync(int orderId, int productId, string userId, string reason);
        Task<List<ReturnRequest>> GetMyReturnsAsync(string userId);

        // ── নতুন: Order Cancel ──
        Task<Order?> GetCancellableOrderAsync(int orderId, string userId);
        Task CancelOrderAsync(Order order, string? reason);
    }
}