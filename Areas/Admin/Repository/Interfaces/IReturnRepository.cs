using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IReturnRepository
    {
        Task<List<ReturnRequest>> GetAllAsync(string? status);
        Task<int> GetPendingCountAsync();
        Task<ReturnRequest?> GetDetailAsync(int id);
        Task<ReturnRequest?> GetWithOrderAsync(int id);
        Task UpdateStatusAsync(ReturnRequest ret, string status, string? adminNote);
    }
}
