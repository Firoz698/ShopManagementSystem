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
    public class HomepageSectionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public HomepageSectionController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db; _imageService = imageService;
        }

        // GET /Admin/HomepageSection
        public async Task<IActionResult> Index()
        {
            var sections = await _db.HomepageSections
                .Include(s => s.Categories).ThenInclude(sc => sc.Category)
                .Include(s => s.Products).ThenInclude(sp => sp.Product)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
            return View(sections);
        }

        // GET /Admin/HomepageSection/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AllCategories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.AllProducts = await _db.Products.Where(p => p.IsActive)
                .Include(p => p.Images).OrderBy(p => p.Name).ToListAsync();
            return View(new HomepageSectionCreateViewModel());
        }

        // POST /Admin/HomepageSection/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomepageSectionCreateViewModel vm)
        {
            var section = new HomepageSection
            {
                Title = vm.Title,
                SectionType = vm.SectionType,
                SubTitle = vm.SubTitle,
                BgColor = vm.BgColor,
                TextColor = vm.TextColor,
                ButtonText = vm.ButtonText,
                ButtonUrl = vm.ButtonUrl,
                IsActive = vm.IsActive,
                SortOrder = vm.SortOrder
            };

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
                section.BannerImageUrl = await _imageService.UploadAsync(vm.BannerImage, "sections");

            _db.HomepageSections.Add(section);
            await _db.SaveChangesAsync();

            // Categories
            foreach (var catId in vm.SelectedCategoryIds)
                _db.HomepageSectionCategories.Add(new HomepageSectionCategory { SectionId = section.Id, CategoryId = catId });

            // Products
            foreach (var prodId in vm.SelectedProductIds)
                _db.HomepageSectionProducts.Add(new HomepageSectionProduct { SectionId = section.Id, ProductId = prodId });

            await _db.SaveChangesAsync();
            TempData["Success"] = "হোমপেজ সেকশন যোগ হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/HomepageSection/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var section = await _db.HomepageSections.FindAsync(id);
            if (section != null) { section.IsActive = !section.IsActive; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // POST /Admin/HomepageSection/MoveUp
        [HttpPost]
        public async Task<IActionResult> MoveUp(int id)
        {
            var section = await _db.HomepageSections.FindAsync(id);
            if (section != null && section.SortOrder > 0) { section.SortOrder--; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // POST /Admin/HomepageSection/MoveDown
        [HttpPost]
        public async Task<IActionResult> MoveDown(int id)
        {
            var section = await _db.HomepageSections.FindAsync(id);
            if (section != null) { section.SortOrder++; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // POST /Admin/HomepageSection/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _db.HomepageSections.FindAsync(id);
            if (section != null)
            {
                if (!string.IsNullOrEmpty(section.BannerImageUrl)) _imageService.Delete(section.BannerImageUrl);
                _db.HomepageSections.Remove(section);
                await _db.SaveChangesAsync();
                TempData["Success"] = "সেকশন মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}
