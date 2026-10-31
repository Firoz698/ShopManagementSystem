using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly ApplicationDbContext _db;

        public ReturnRepository(ApplicationDbContext db)
        {
            _db = db;
        }

 // Filter return request — user, product, order 
        public async Task<List<ReturnRequest>> GetAllAsync(string? status)
        {
            var query = _db.ReturnRequests
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

 // Pending status count (badge )
        public async Task<int> GetPendingCountAsync()
        {
            return await _db.ReturnRequests.CountAsync(r => r.Status == "Pending");
        }

 // Detail page — product images order details 
        public async Task<ReturnRequest?> GetDetailAsync(int id)
        {
            return await _db.ReturnRequests
                .Include(r => r.User)
                .Include(r => r.Product).ThenInclude(p => p!.Images)
                .Include(r => r.Order).ThenInclude(o => o!.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

 // Status update — order 
        public async Task<ReturnRequest?> GetWithOrderAsync(int id)
        {
            return await _db.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

 // Status, adminNote order status update 
        public async Task UpdateStatusAsync(ReturnRequest ret, string status, string? adminNote)
        {
            ret.Status = status;
            ret.AdminNote = adminNote ?? ret.AdminNote;
            ret.UpdatedAt = DateTime.Now;

            if (ret.Order != null)
            {
                ret.Order.Status = status switch
                {
                    "Approved" => "Return Approved",
                    "Refunded" => "Refunded",
                    "Rejected" => "Return Rejected",
                    _ => ret.Order.Status
                };
            }

            await _db.SaveChangesAsync();
        }
    }
}

