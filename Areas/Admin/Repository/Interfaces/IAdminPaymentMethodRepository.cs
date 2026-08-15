using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminPaymentMethodRepository
    {
        Task<List<PaymentMethodSetting>> GetAllAsync();
        Task<List<PaymentMethodSetting>> GetActiveAsync();
        Task<PaymentMethodSetting?> GetByIdAsync(int id);
        Task CreateAsync(PaymentMethodSetting method);
        Task UpdateAsync(PaymentMethodSetting method);
        Task DeleteAsync(int id);
        Task ToggleActiveAsync(int id);
    }
}