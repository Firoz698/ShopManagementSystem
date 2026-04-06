using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext         _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db; _userManager = userManager;
        }

        private string UserId => _userManager.GetUserId(User)!;

        public async Task<IActionResult> Index()
        {
            var items = await _db.Wishlists
                .Where(w => w.UserId == UserId)
                .Include(w => w.Product).ThenInclude(p => p!.Images)
                .Include(w => w.Product).ThenInclude(p => p!.Category)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var item = await _db.Wishlists.FirstOrDefaultAsync(w => w.UserId == UserId && w.ProductId == productId);
            if (item == null)
            {
                _db.Wishlists.Add(new Wishlist { UserId = UserId, ProductId = productId });
                TempData["Success"] = "উইশলিস্টে যোগ করা হয়েছে।";
            }
            else
            {
                _db.Wishlists.Remove(item);
                TempData["Success"] = "উইশলিস্ট থেকে সরানো হয়েছে।";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Detail", "Product", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _db.Wishlists.FirstOrDefaultAsync(w => w.Id == id && w.UserId == UserId);
            if (item != null) { _db.Wishlists.Remove(item); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}
