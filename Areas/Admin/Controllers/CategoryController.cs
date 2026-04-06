using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService        _imageService;

        public CategoryController(ApplicationDbContext db, IImageService imageService)
        {
            _db           = db;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create() => View(new Category());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            if (image != null && image.Length > 0)
                model.ImageUrl = await _imageService.UploadAsync(image, "categories");

            model.Slug      = model.Name.ToLower().Replace(" ", "-");
            model.CreatedAt = DateTime.Now;
            _db.Categories.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "ক্যাটাগরি যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model, IFormFile? image)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            cat.Name     = model.Name;
            cat.IsActive = model.IsActive;
            cat.Slug     = model.Name.ToLower().Replace(" ", "-");

            if (image != null && image.Length > 0)
            {
                if (!string.IsNullOrEmpty(cat.ImageUrl)) _imageService.Delete(cat.ImageUrl);
                cat.ImageUrl = await _imageService.UploadAsync(image, "categories");
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "ক্যাটাগরি আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat != null)
            {
                if (!string.IsNullOrEmpty(cat.ImageUrl)) _imageService.Delete(cat.ImageUrl);
                _db.Categories.Remove(cat);
                await _db.SaveChangesAsync();
                TempData["Success"] = "ক্যাটাগরি মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}
