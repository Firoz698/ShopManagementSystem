using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepo;

        public DashboardController(IDashboardRepository dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalProducts = await _dashboardRepo.GetTotalProductsAsync(),
                TotalOrders = await _dashboardRepo.GetTotalOrdersAsync(),
                TotalUsers = await _dashboardRepo.GetTotalUsersAsync(),
                PendingOrders = await _dashboardRepo.GetPendingOrdersAsync(),
                TotalRevenue = await _dashboardRepo.GetTotalRevenueAsync(),
                RecentOrders = await _dashboardRepo.GetRecentOrdersAsync(10),
                LowStockItems = await _dashboardRepo.GetLowStockItemsAsync(5, 10)
            };
            return View(vm);
        }
    }
}