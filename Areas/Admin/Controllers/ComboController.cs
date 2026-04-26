using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ComboController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public ComboController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db; _imageService = imageService;
        }

        // GET /Admin/Combo
        public async Task<IActionResult> Index()
        {
            var combos = await _db.ComboOffers
                .Include(c => c.Items).ThenInclude(ci => ci.Product)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(combos);
        }

        // GET /Admin/Combo/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(new ComboCreateViewModel());
        }

        // POST /Admin/Combo/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComboCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _db.Products.Where(p => p.IsActive).ToListAsync();
                return View(vm);
            }

            var combo = new ComboOffer
            {
                Title = vm.Title,
                Description = vm.Description,
                ComboPrice = vm.ComboPrice,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                IsActive = vm.IsActive,
                SortOrder = vm.SortOrder
            };

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
                combo.BannerImageUrl = await _imageService.UploadAsync(vm.BannerImage, "combos");

            _db.ComboOffers.Add(combo);
            await _db.SaveChangesAsync();

            // Items যোগ করো
            decimal originalTotal = 0;
            foreach (var entry in vm.Products.Where(p => p.ProductId > 0))
            {
                var product = await _db.Products.FindAsync(entry.ProductId);
                if (product == null) continue;

                _db.ComboItems.Add(new ComboItem
                {
                    ComboId = combo.Id,
                    ProductId = entry.ProductId,
                    Quantity = entry.Quantity
                });
                originalTotal += (product.DiscountPrice ?? product.Price) * entry.Quantity;
            }

            combo.OriginalPrice = originalTotal;
            await _db.SaveChangesAsync();

            TempData["Success"] = "কম্বো অফার যোগ হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Combo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var combo = await _db.ComboOffers
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (combo == null) return NotFound();

            ViewBag.Products = await _db.Products.Where(p => p.IsActive)
                .Include(p => p.Images).OrderBy(p => p.Name).ToListAsync();
            ViewBag.ExistingBanner = combo.BannerImageUrl;

            var vm = new ComboCreateViewModel
            {
                Title = combo.Title,
                Description = combo.Description,
                ComboPrice = combo.ComboPrice,
                StartDate = combo.StartDate,
                EndDate = combo.EndDate,
                IsActive = combo.IsActive,
                SortOrder = combo.SortOrder,
                Products = combo.Items.Select(i => new ComboProductEntry
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
            return View(vm);
        }

        // POST /Admin/Combo/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComboCreateViewModel vm)
        {
            var combo = await _db.ComboOffers.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
            if (combo == null) return NotFound();

            combo.Title = vm.Title;
            combo.Description = vm.Description;
            combo.ComboPrice = vm.ComboPrice;
            combo.StartDate = vm.StartDate;
            combo.EndDate = vm.EndDate;
            combo.IsActive = vm.IsActive;
            combo.SortOrder = vm.SortOrder;

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(combo.BannerImageUrl)) _imageService.Delete(combo.BannerImageUrl);
                combo.BannerImageUrl = await _imageService.UploadAsync(vm.BannerImage, "combos");
            }

            // Items পুনরায় তৈরি
            _db.ComboItems.RemoveRange(combo.Items);
            decimal originalTotal = 0;
            foreach (var entry in vm.Products.Where(p => p.ProductId > 0))
            {
                var product = await _db.Products.FindAsync(entry.ProductId);
                if (product == null) continue;
                _db.ComboItems.Add(new ComboItem { ComboId = id, ProductId = entry.ProductId, Quantity = entry.Quantity });
                originalTotal += (product.DiscountPrice ?? product.Price) * entry.Quantity;
            }
            combo.OriginalPrice = originalTotal;

            await _db.SaveChangesAsync();
            TempData["Success"] = "কম্বো আপডেট হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/Combo/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var combo = await _db.ComboOffers.FindAsync(id);
            if (combo != null) { combo.IsActive = !combo.IsActive; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // POST /Admin/Combo/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var combo = await _db.ComboOffers.FindAsync(id);
            if (combo != null)
            {
                if (!string.IsNullOrEmpty(combo.BannerImageUrl)) _imageService.Delete(combo.BannerImageUrl);
                _db.ComboOffers.Remove(combo);
                await _db.SaveChangesAsync();
                TempData["Success"] = "কম্বো মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}
