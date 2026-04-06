using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext              _db;
        private readonly UserManager<ApplicationUser>      _userManager;

        public ProductController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db          = db;
            _userManager = userManager;
        }

        // GET /Product
        public async Task<IActionResult> Index(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1)
        {
            const int pageSize = 12;

            var query = _db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortBy switch
            {
                "price_asc"  => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name"       => query.OrderBy(p => p.Name),
                _            => query.OrderByDescending(p => p.CreatedAt)
            };

            var total    = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new ProductListViewModel
            {
                Products   = products,
                Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync(),
                CategoryId = categoryId,
                SearchTerm = search,
                MinPrice   = minPrice,
                MaxPrice   = maxPrice,
                SortBy     = sortBy,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
            return View(vm);
        }

        // GET /Product/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var vm = new ProductDetailViewModel
            {
                Product      = product,
                Reviews      = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
                AvgRating    = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
                IsInWishlist = userId != null && await _db.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == id),
                UserReviewed = userId != null && product.Reviews.Any(r => r.UserId == userId),
                RelatedItems = await _db.Products
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsActive)
                    .Include(p => p.Images)
                    .Take(4)
                    .ToListAsync()
            };
            return View(vm);
        }

        // POST /Product/AddReview
        [HttpPost, Authorize]
        public async Task<IActionResult> AddReview(ReviewViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction("Detail", new { id = vm.ProductId });

            var userId = _userManager.GetUserId(User)!;
            var exists = await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == vm.ProductId);
            if (!exists)
            {
                _db.Reviews.Add(new Review
                {
                    UserId    = userId,
                    ProductId = vm.ProductId,
                    Rating    = vm.Rating,
                    Comment   = vm.Comment
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "আপনার রিভিউ যোগ করা হয়েছে।";
            }
            return RedirectToAction("Detail", new { id = vm.ProductId });
        }
    }
}
