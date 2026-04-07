using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService        _imageService;

        public ProductController(ApplicationDbContext db, IImageService imageService)
        {
            _db           = db;
            _imageService = imageService;
        }

        // GET /Admin/Product
        public async Task<IActionResult> Index(string? search, int? categoryId, bool? isActive, int page = 1)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            // 📂 Category Filter
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // 🔄 Active Filter
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            // 📊 Total Count (pagination এর জন্য)
            int totalCount = await query.CountAsync();

            int pageSize = 10;

            // ❗ page validation
            if (page < 1) page = 1;

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (page > totalPages && totalPages > 0)
                page = totalPages;

            // 📥 Data Fetch with Pagination
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🎯 ViewBag Data
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.IsActive = isActive;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        // GET /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            return View(new ProductCreateViewModel());
        }

        // POST /Admin/Product/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
                return View(vm);
            }

            var product = new Product
            {
                Name         = vm.Name,
                CategoryId   = vm.CategoryId,
                Price        = vm.Price,
                DiscountPrice = vm.DiscountPrice,
                Stock        = vm.Stock,
                Description  = vm.Description,
                IsActive     = vm.IsActive,
                Slug         = vm.Name.ToLower().Replace(" ", "-").Replace("'", "")
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            // Upload images
            if (vm.Images != null && vm.Images.Any())
            {
                bool isFirst = true;
                foreach (var file in vm.Images)
                {
                    if (file.Length > 0)
                    {
                        var url = await _imageService.UploadAsync(file, "products");
                        _db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl  = url,
                            IsPrimary = isFirst
                        });
                        isFirst = false;
                    }
                }
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "প্রোডাক্ট সফলভাবে যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.ExistingImages = product.Images.ToList();

            var vm = new ProductCreateViewModel
            {
                Name          = product.Name,
                CategoryId    = product.CategoryId,
                Price         = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock         = product.Stock,
                Description   = product.Description,
                IsActive      = product.IsActive
            };
            return View(vm);
        }

        // POST /Admin/Product/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel vm)
        {
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories     = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
                ViewBag.ExistingImages = product.Images.ToList();
                return View(vm);
            }

            product.Name          = vm.Name;
            product.CategoryId    = vm.CategoryId;
            product.Price         = vm.Price;
            product.DiscountPrice = vm.DiscountPrice;
            product.Stock         = vm.Stock;
            product.Description   = vm.Description;
            product.IsActive      = vm.IsActive;
            product.Slug          = vm.Name.ToLower().Replace(" ", "-").Replace("'", "");

            // Remove selected images
            if (vm.RemoveImageIds != null && vm.RemoveImageIds.Any())
            {
                var toRemove = product.Images.Where(i => vm.RemoveImageIds.Contains(i.Id)).ToList();
                foreach (var img in toRemove) { _imageService.Delete(img.ImageUrl); _db.ProductImages.Remove(img); }
            }

            // Add new images
            if (vm.Images != null && vm.Images.Any())
            {
                bool noPrimary = !product.Images.Any(i => i.IsPrimary);
                bool isFirst   = noPrimary;
                foreach (var file in vm.Images.Where(f => f.Length > 0))
                {
                    var url = await _imageService.UploadAsync(file, "products");
                    _db.ProductImages.Add(new ProductImage { ProductId = id, ImageUrl = url, IsPrimary = isFirst });
                    isFirst = false;
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "প্রোডাক্ট আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/Product/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST /Admin/Product/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                foreach (var img in product.Images) _imageService.Delete(img.ImageUrl);
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "প্রোডাক্ট মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}
