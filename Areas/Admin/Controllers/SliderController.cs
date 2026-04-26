using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ISliderRepository _sliderRepo;
        private readonly IImageService _imageService;

        public SliderController(ISliderRepository sliderRepo, IImageService imageService)
        {
            _sliderRepo = sliderRepo;
            _imageService = imageService;
        }

        // GET /Admin/Slider
        public async Task<IActionResult> Index()
        {
            var sliders = await _sliderRepo.GetAllOrderedAsync();
            return View(sliders);
        }

        // GET /Admin/Slider/Create
        [HttpGet]
        public IActionResult Create() => View(new Slider());

        // POST /Admin/Slider/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider model, IFormFile? image)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "স্লাইডার ইমেজ আপলোড করুন।");
                return View(model);
            }

            model.ImageUrl = await _imageService.UploadAsync(image, "sliders");
            model.CreatedAt = DateTime.Now;

            await _sliderRepo.AddAsync(model);
            await _sliderRepo.SaveAsync();

            TempData["Success"] = "স্লাইডার যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Slider/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _sliderRepo.GetByIdAsync(id);
            if (slider == null) return NotFound();
            return View(slider);
        }

        // POST /Admin/Slider/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider model, IFormFile? image)
        {
            var slider = await _sliderRepo.GetByIdAsync(id);
            if (slider == null) return NotFound();

            slider.Title = model.Title;
            slider.SubTitle = model.SubTitle;
            slider.ButtonText = model.ButtonText;
            slider.ButtonUrl = model.ButtonUrl;
            slider.IsActive = model.IsActive;
            slider.SortOrder = model.SortOrder;

            if (image != null && image.Length > 0)
            {
                _imageService.Delete(slider.ImageUrl);
                slider.ImageUrl = await _imageService.UploadAsync(image, "sliders");
            }

            await _sliderRepo.UpdateAsync(slider);
            await _sliderRepo.SaveAsync();

            TempData["Success"] = "স্লাইডার আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/Slider/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _sliderRepo.GetByIdAsync(id);
            if (slider != null)
            {
                _imageService.Delete(slider.ImageUrl);
                await _sliderRepo.DeleteAsync(slider);
                await _sliderRepo.SaveAsync();
                TempData["Success"] = "স্লাইডার মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}