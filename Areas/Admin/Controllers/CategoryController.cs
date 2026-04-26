using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IImageService _imageService;

        public CategoryController(ICategoryRepository categoryRepo, IImageService imageService)
        {
            _categoryRepo = categoryRepo;
            _imageService = imageService;
        }

        // GET /Admin/Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAllWithProductsAsync();
            return View(categories);
        }

        // GET /Admin/Category/Create
        [HttpGet]
        public IActionResult Create() => View(new Category());

        // POST /Admin/Category/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            if (image != null && image.Length > 0)
                model.ImageUrl = await _imageService.UploadAsync(image, "categories");

            model.Slug = model.Name.ToLower().Replace(" ", "-");
            model.CreatedAt = DateTime.Now;

            await _categoryRepo.AddAsync(model);
            await _categoryRepo.SaveAsync();

            TempData["Success"] = "ক্যাটাগরি যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _categoryRepo.GetByIdAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // POST /Admin/Category/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model, IFormFile? image)
        {
            var cat = await _categoryRepo.GetByIdAsync(id);
            if (cat == null) return NotFound();

            cat.Name = model.Name;
            cat.IsActive = model.IsActive;
            cat.Slug = model.Name.ToLower().Replace(" ", "-");

            if (image != null && image.Length > 0)
            {
                if (!string.IsNullOrEmpty(cat.ImageUrl))
                    _imageService.Delete(cat.ImageUrl);

                cat.ImageUrl = await _imageService.UploadAsync(image, "categories");
            }

            await _categoryRepo.UpdateAsync(cat);
            await _categoryRepo.SaveAsync();

            TempData["Success"] = "ক্যাটাগরি আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/Category/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _categoryRepo.GetByIdAsync(id);
            if (cat != null)
            {
                if (!string.IsNullOrEmpty(cat.ImageUrl))
                    _imageService.Delete(cat.ImageUrl);

                await _categoryRepo.DeleteAsync(cat);
                await _categoryRepo.SaveAsync();

                TempData["Success"] = "ক্যাটাগরি মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}