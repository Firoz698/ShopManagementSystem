using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminOrderRepository
    {
        Task<List<Order>> GetFilteredOrdersAsync(string? status);
        Task<int> GetPendingOrdersCountAsync();
        Task<Order?> GetOrderWithUserAsync(int orderId);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId);
        Task<Order?> GetUserOrderAsync(int orderId, string userId);
        Task AddReturnRequestAsync(int orderId, int productId, string userId, string reason);
        Task<Order?> GetOrderDetailAsync(int id);
        Task<Order?> GetByIdAsync(int id);
        Task UpdateStatusAsync(Order order, string status);
        Task DeleteAsync(Order order);
    }
}
