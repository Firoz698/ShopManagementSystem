using ShopManagementSystem.Models;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IPaymentMethodRepository
    {
        Task<List<PaymentMethodSetting>> GetActiveAsync();
    }
}