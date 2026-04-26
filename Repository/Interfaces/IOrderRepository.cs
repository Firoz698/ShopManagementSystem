using ShopManagementSystem.Models;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IOrderRepository
    {
        // Review
        Task<Order?> GetCompletedOrderAsync(int orderId, string userId);
        Task<bool> HasUserReviewedProductAsync(string userId, int productId);
        Task AddReviewAsync(string userId, int productId, int rating, string? comment);

        // Return Request
        Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId);
        Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId);
        Task AddReturnRequestAsync(int orderId, int productId, string userId, string reason);

        // My Returns
        Task<List<ReturnRequest>> GetMyReturnsAsync(string userId);
    }
}
