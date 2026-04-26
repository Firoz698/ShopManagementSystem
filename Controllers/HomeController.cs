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
            var now = DateTime.Now;

            // Active Sections (sorted)
            var sections = await _db.HomepageSections
                .Where(s => s.IsActive)
                .Include(s => s.Categories).ThenInclude(sc => sc.Category)
                .Include(s => s.Products).ThenInclude(sp => sp.Product)
                    .ThenInclude(p => p!.Images)
                .Include(s => s.Products).ThenInclude(sp => sp.Product)
                    .ThenInclude(p => p!.Sizes)
                .Include(s => s.Products).ThenInclude(sp => sp.Product)
                    .ThenInclude(p => p!.Category)   // ← যোগ করা হয়েছে
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            // Active Combos
            var combos = await _db.ComboOffers
                .Where(c => c.IsActive &&
                            (c.StartDate == null || c.StartDate <= now) &&
                            (c.EndDate == null || c.EndDate >= now))
                .Include(c => c.Items).ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Images)
                .OrderBy(c => c.SortOrder)
                .Take(6)
                .ToListAsync();

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
                    .Include(p => p.Sizes)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync(),

                ComboOffers = combos,
                Sections = sections
            };

            return View(vm);
        }

        public IActionResult Error() => View();
    }
}