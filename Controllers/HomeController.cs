using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                Sliders = await _db.Sliders
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .ToListAsync(),

                Categories = await _db.Categories
                    .Where(c => c.IsActive)
                    .Take(8)
                    .ToListAsync(),

                Products = await _db.Products
                    .Where(p => p.IsActive)
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync()
            };
            return View(vm);
        }

        public IActionResult Error() => View();
    }
}
