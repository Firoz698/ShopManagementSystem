using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Controllers
{
    public class ComboController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComboController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db; _userManager = userManager;
        }

        private string? UserId => _userManager.GetUserId(User);

        // GET /Combo — সব active combo
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var combos = await _db.ComboOffers
                .Where(c => c.IsActive &&
                            (c.StartDate == null || c.StartDate <= now) &&
                            (c.EndDate == null || c.EndDate >= now))
                .Include(c => c.Items).ThenInclude(ci => ci.Product).ThenInclude(p => p!.Images)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            return View(combos);
        }

        // GET /Combo/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var now = DateTime.Now;
            var combo = await _db.ComboOffers
                .Include(c => c.Items).ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Images)
                .Include(c => c.Items).ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive &&
                    (c.StartDate == null || c.StartDate <= now) &&
                    (c.EndDate == null || c.EndDate >= now));

            if (combo == null) return NotFound();
            return View(combo);
        }

        // POST /Combo/AddToCart/5 — পুরো combo কার্টে যোগ করো
        [HttpPost, Authorize]
        public async Task<IActionResult> AddToCart(int id)
        {
            var now = DateTime.Now;
            var combo = await _db.ComboOffers
                .Include(c => c.Items).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive &&
                    (c.EndDate == null || c.EndDate >= now));

            if (combo == null)
            {
                TempData["Error"] = "কম্বো অফার পাওয়া যায়নি।";
                return RedirectToAction("Index");
            }

            // Stock check
            foreach (var item in combo.Items)
            {
                if (item.Product!.Stock < item.Quantity)
                {
                    TempData["Error"] = $"'{item.Product.Name}' এর পর্যাপ্ত স্টক নেই।";
                    return RedirectToAction("Detail", new { id });
                }
            }

            // প্রতিটি product কার্টে যোগ করো
            foreach (var item in combo.Items)
            {
                var cart = await _db.Carts.FirstOrDefaultAsync(c =>
                    c.UserId == UserId && c.ProductId == item.ProductId && c.ProductSizeId == null);

                if (cart == null)
                {
                    _db.Carts.Add(new Cart
                    {
                        UserId = UserId!,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
                else
                {
                    cart.Quantity = Math.Min(cart.Quantity + item.Quantity, item.Product!.Stock);
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{combo.Title}' কম্বো কার্টে যোগ হয়েছে!";
            return RedirectToAction("Index", "Cart");
        }
    }
}
