using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IWishlistRepository wishlistRepo, UserManager<ApplicationUser> userManager)
        {
            _wishlistRepo = wishlistRepo;
            _userManager = userManager;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // GET /Wishlist
        public async Task<IActionResult> Index()
        {
            var items = await _wishlistRepo.GetWishlistItemsAsync(UserId);
            return View(items);
        }

        // POST /Wishlist/Toggle - removes if exists, adds if not
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var item = await _wishlistRepo.GetByProductIdAsync(UserId, productId);

            if (item == null)
            {
                await _wishlistRepo.AddAsync(UserId, productId);
                TempData["Success"] = "Added to your wishlist.";
            }
            else
            {
                await _wishlistRepo.RemoveAsync(item);
                TempData["Success"] = "Removed from your wishlist.";
            }

            return RedirectToAction("Detail", "Product", new { id = productId });
        }

        // POST /Wishlist/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _wishlistRepo.GetByIdAsync(id, UserId);
            if (item != null)
                await _wishlistRepo.RemoveAsync(item);

            return RedirectToAction("Index");
        }
    }
}