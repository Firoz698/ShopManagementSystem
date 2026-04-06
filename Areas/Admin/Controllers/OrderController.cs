using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            ViewBag.Status       = status;
            ViewBag.StatusList   = new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            ViewBag.PendingCount = await _db.Orders.CountAsync(o => o.Status == "Pending");

            return View(await query.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            ViewBag.StatusList = new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status    = status;
                order.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"অর্ডার #{id} স্ট্যাটাস '{status}' করা হয়েছে।";
            }
            return RedirectToAction("Detail", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order != null) { _db.Orders.Remove(order); await _db.SaveChangesAsync(); }
            TempData["Success"] = "অর্ডার মুছে ফেলা হয়েছে।";
            return RedirectToAction("Index");
        }
    }
}
