using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalProducts = await _db.Products.CountAsync(),
                TotalOrders   = await _db.Orders.CountAsync(),
                TotalUsers    = await _db.Users.CountAsync(),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending"),
                TotalRevenue  = await _db.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount),
                RecentOrders = await _db.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),
                LowStockItems = await _db.Products
                    .Where(p => p.Stock <= 5)
                    .Take(10)
                    .ToListAsync()
            };
            return View(vm);
        }
    }
}
