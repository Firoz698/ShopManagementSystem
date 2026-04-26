using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IAdminProductRepository _productRepo;
        private readonly IImageService _imageService;

        public ProductController(IAdminProductRepository productRepo, IImageService imageService)
        {
            _productRepo = productRepo;
            _imageService = imageService;
        }

        // GET /Admin/Product
        public async Task<IActionResult> Index(string? search, int? categoryId, bool? isActive)
        {
            var products = await _productRepo.GetFilteredProductsAsync(search, categoryId, isActive);

            ViewBag.Categories = await _productRepo.GetActiveCategoriesAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.IsActive = isActive;

            return View(products);
        }

        // GET /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _productRepo.GetActiveCategoriesAsync(), "Id", "Name");
            return View(new ProductCreateViewModel());
        }

        // POST /Admin/Product/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _productRepo.GetAllCategoriesAsync(), "Id", "Name");
                return View(vm);
            }

            var product = await _productRepo.CreateProductAsync(vm);

            if (vm.Images != null && vm.Images.Any())
                await _productRepo.AddProductImagesAsync(product.Id, vm.Images, _imageService);

            if (vm.Sizes != null && vm.Sizes.Any())
                await _productRepo.AddProductSizesAsync(product.Id, vm.Sizes);

            TempData["Success"] = "প্রোডাক্ট সফলভাবে যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepo.GetProductWithImagesAndSizesAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(
                await _productRepo.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            ViewBag.ExistingImages = product.Images.ToList();

            var vm = new ProductCreateViewModel
            {
                Name = product.Name,
                ProductCode = product.ProductCode,
                CategoryId = product.CategoryId,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                PurchasePrice = product.PurchasePrice,
                ProductVat = product.ProductVat,
                IsDiscountFixed = product.IsDiscountFixed,
                DiscountStartDate = product.DiscountStartDate,
                DiscountEndDate = product.DiscountEndDate,
                Stock = product.Stock,
                Description = product.Description,
                IsActive = product.IsActive,
                Sizes = product.Sizes.Select(s => new ProductSizeEntry
                {
                    Id = s.Id,
                    SizeName = s.SizeName,
                    SizeCode = s.SizeCode,
                    SalesPrice = s.SalesPrice,
                    Stock = s.Stock,
                    IsActive = s.IsActive
                }).ToList()
            };
            return View(vm);
        }

        // POST /Admin/Product/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel vm, int? PrimaryImageId)
        {
            var product = await _productRepo.GetProductWithImagesAndSizesAsync(id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _productRepo.GetAllCategoriesAsync(), "Id", "Name");
                ViewBag.ExistingImages = product.Images.ToList();
                return View(vm);
            }

            await _productRepo.UpdateProductAsync(product, vm);
            await _productRepo.UpdatePrimaryImageAsync(product, PrimaryImageId);

            if (vm.RemoveImageIds != null && vm.RemoveImageIds.Any())
                await _productRepo.RemoveImagesAsync(product, vm.RemoveImageIds, _imageService);

            if (vm.Images != null && vm.Images.Any())
                await _productRepo.AddNewImagesAsync(id, product, vm.Images, _imageService);

            if (vm.Sizes != null)
                await _productRepo.UpdateSizesAsync(id, product, vm.Sizes);

            TempData["Success"] = "প্রোডাক্ট আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/Product/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product != null)
                await _productRepo.ToggleActiveAsync(product);

            return RedirectToAction("Index");
        }

        // POST /Admin/Product/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product != null)
            {
                await _productRepo.DeleteProductAsync(product, _imageService);
                TempData["Success"] = "প্রোডাক্ট মুছে ফেলা হয়েছে।";
            }
            return RedirectToAction("Index");
        }
    }
}