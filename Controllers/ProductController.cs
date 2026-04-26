using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IProductRepository productRepo, UserManager<ApplicationUser> userManager)
        {
            _productRepo = productRepo;
            _userManager = userManager;
        }

        // GET /Product
        public async Task<IActionResult> Index(int? categoryId, string? search,decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1)
        {
            const int pageSize = 12;

            var (products, total) = await _productRepo.GetProductsAsync(categoryId, search, minPrice, maxPrice, sortBy, page, pageSize);

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = await _productRepo.GetActiveCategoriesAsync(),
                CategoryId = categoryId,
                SearchTerm = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return View(vm);
        }

        // GET /Product/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productRepo.GetProductDetailAsync(id);
            if (product == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
                AvgRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
                IsInWishlist = userId != null && await _productRepo.IsInWishlistAsync(userId, id),
                UserReviewed = userId != null && await _productRepo.HasUserReviewedAsync(userId, id),
                RelatedItems = await _productRepo.GetRelatedProductsAsync(product.CategoryId, id)
            };

            return View(vm);
        }

        // POST /Product/AddReview
        [HttpPost, Authorize]
        public async Task<IActionResult> AddReview(ReviewViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Detail", new { id = vm.ProductId });

            var userId = _userManager.GetUserId(User)!;
            await _productRepo.AddReviewAsync(userId, vm);

            TempData["Success"] = "আপনার রিভিউ যোগ করা হয়েছে।";
            return RedirectToAction("Detail", new { id = vm.ProductId });
        }
    }
}