using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository
{
    public class AdminPaymentMethodRepository : IAdminPaymentMethodRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminPaymentMethodRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentMethodSetting>> GetAllAsync()
        {
            return await _context.PaymentMethodSettings
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.MethodName)
                .ToListAsync();
        }

        public async Task<List<PaymentMethodSetting>> GetActiveAsync()
        {
            return await _context.PaymentMethodSettings
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PaymentMethodSetting?> GetByIdAsync(int id)
        {
            return await _context.PaymentMethodSettings.FindAsync(id);
        }

        public async Task CreateAsync(PaymentMethodSetting method)
        {
            method.CreatedAt = DateTime.Now;
            _context.PaymentMethodSettings.Add(method);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PaymentMethodSetting method)
        {
            var existing = await _context.PaymentMethodSettings.FindAsync(method.Id);
            if (existing == null) return;

            existing.MethodName = method.MethodName;
            existing.AccountType = method.AccountType;
            existing.AccountNumber = method.AccountNumber;
            existing.AccountHolderName = method.AccountHolderName;
            existing.BankName = method.BankName;
            existing.BranchName = method.BranchName;
            existing.RoutingOrSwift = method.RoutingOrSwift;
            existing.Instructions = method.Instructions;
            existing.LogoUrl = method.LogoUrl;
            existing.IsActive = method.IsActive;
            existing.DisplayOrder = method.DisplayOrder;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.PaymentMethodSettings.FindAsync(id);
            if (existing != null)
            {
                _context.PaymentMethodSettings.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleActiveAsync(int id)
        {
            var existing = await _context.PaymentMethodSettings.FindAsync(id);
            if (existing != null)
            {
                existing.IsActive = !existing.IsActive;
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}