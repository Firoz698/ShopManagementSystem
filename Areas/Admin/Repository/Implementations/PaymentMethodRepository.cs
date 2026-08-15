using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Repository.Implementations
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentMethodRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PaymentMethodSetting>> GetActiveAsync()
        {
            return await _db.PaymentMethodSettings
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }
    }
}