using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalProductsAsync();
        Task<int> GetTotalOrdersAsync();
        Task<int> GetTotalUsersAsync();
        Task<int> GetPendingOrdersAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<List<Order>> GetRecentOrdersAsync(int take = 10);
        Task<List<Product>> GetLowStockItemsAsync(int threshold = 5, int take = 10);
    }
}
