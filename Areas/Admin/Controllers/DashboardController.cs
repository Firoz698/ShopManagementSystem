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

        public async Task<IActionResult> Index(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var today = DateTime.Today;
            var now = DateTime.Now;

            // ── Default: আজকের তারিখ ─────────────────────────────────────────
            var filterStart = startDate ?? today;
            var filterEnd = endDate?.AddDays(1).AddSeconds(-1) ?? today.AddDays(1).AddSeconds(-1);

            ViewBag.StartDate = filterStart.ToString("yyyy-MM-dd");
            ViewBag.EndDate = (endDate ?? today).ToString("yyyy-MM-dd");

            // ── আজকের Stats ──────────────────────────────────────────────────
            var todayOrders = await _db.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < today.AddDays(1))
                .ToListAsync();

            var todayRevenue = todayOrders
                .Where(o => o.Status == "Completed" || o.Status == "Processing" || o.Status == "Shipped")
                .Sum(o => o.TotalAmount);

            // ── Filter Range Stats ────────────────────────────────────────────
            var filteredOrders = await _db.Orders
                .Where(o => o.OrderDate >= filterStart && o.OrderDate <= filterEnd)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var filteredRevenue = filteredOrders
                .Where(o => o.Status == "Completed" || o.Status == "Processing" || o.Status == "Shipped")
                .Sum(o => o.TotalAmount);

            var filteredProfit = filteredOrders
                .Where(o => o.Status == "Completed" || o.Status == "Processing" || o.Status == "Shipped")
                .SelectMany(o => o.OrderDetails)
                .Sum(od => od.UnitPrice * od.Quantity); // simplified profit

            // ── Overall Stats ─────────────────────────────────────────────────
            var vm = new AdminDashboardViewModel
            {
                TotalProducts = await _db.Products.CountAsync(),
                TotalOrders = await _db.Orders.CountAsync(),
                TotalUsers = await _db.Users.CountAsync(),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending"),
                TotalRevenue = await _db.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount),
                RecentOrders = filteredOrders.Take(20).ToList(),
                LowStockItems = await _db.Products.Where(p => p.Stock <= 5).Take(10).ToListAsync(),

                // ── আজকের তথ্য
                TodayOrderCount = todayOrders.Count,
                TodayRevenue = todayRevenue,
                TodayNewUsers = await _db.Users.CountAsync(u => u.CreatedAt >= today),

                // ── Filter range তথ্য
                FilteredOrderCount = filteredOrders.Count,
                FilteredRevenue = filteredRevenue,
                FilteredPending = filteredOrders.Count(o => o.Status == "Pending"),
                FilteredCompleted = filteredOrders.Count(o => o.Status == "Completed"),
                FilteredCancelled = filteredOrders.Count(o => o.Status == "Cancelled"),
                FilteredOrders = filteredOrders
            };

            return View(vm);
        }
    }
}
