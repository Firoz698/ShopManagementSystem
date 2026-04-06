using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService        _imageService;

        public SliderController(ApplicationDbContext db, IImageService imageService)
        {
            _db           = db;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index() =>
            View(await _db.Sliders.OrderBy(s => s.SortOrder).ToListAsync());

        [HttpGet]
        public IActionResult Create() => View(new Slider());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider model, IFormFile? image)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "স্লাইডার ইমেজ আপলোড করুন।");
                return View(model);
            }

            model.ImageUrl  = await _imageService.UploadAsync(image, "sliders");
            model.CreatedAt = DateTime.Now;
            _db.Sliders.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "স্লাইডার যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _db.Sliders.FindAsync(id);
            if (slider == null) return NotFound();
            return View(slider);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider model, IFormFile? image)
        {
            var slider = await _db.Sliders.FindAsync(id);
            if (slider == null) return NotFound();

            slider.Title      = model.Title;
            slider.SubTitle   = model.SubTitle;
            slider.ButtonText = model.ButtonText;
            slider.ButtonUrl  = model.ButtonUrl;
            slider.IsActive   = model.IsActive;
            slider.SortOrder  = model.SortOrder;

            if (image != null && image.Length > 0)
            {
                _imageService.Delete(slider.ImageUrl);
                slider.ImageUrl = await _imageService.UploadAsync(image, "sliders");
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "স্লাইডার আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _db.Sliders.FindAsync(id);
            if (slider != null)
            {
                _imageService.Delete(slider.ImageUrl);
                _db.Sliders.Remove(slider);
                await _db.SaveChangesAsync();
                TempData["Success"] = "স্লাইডার মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}
