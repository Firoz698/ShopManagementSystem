using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class AdminProductRepository : IAdminProductRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Index ─────────────────────────────────────────────────────────────

        // Search, category ও active filter সহ product list
        public async Task<List<Product>> GetFilteredProductsAsync(string? search, int? categoryId, bool? isActive)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.ProductCode != null && p.ProductCode.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        // শুধু active category গুলো (Index/Create dropdown)
        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _db.Categories.Where(c => c.IsActive).ToListAsync();
        }

        // সব category (Edit dropdown)
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _db.Categories.ToListAsync();
        }

        // ── Create ────────────────────────────────────────────────────────────

        // ViewModel থেকে Product তৈরি করে save করে
        public async Task<Product> CreateProductAsync(ProductCreateViewModel vm)
        {
            var product = new Product
            {
                Name = vm.Name,
                ProductCode = vm.ProductCode,
                CategoryId = vm.CategoryId,
                Price = vm.Price,
                DiscountPrice = vm.DiscountPrice,
                PurchasePrice = vm.PurchasePrice,
                ProductVat = vm.ProductVat,
                IsDiscountFixed = vm.IsDiscountFixed,
                DiscountStartDate = vm.DiscountStartDate,
                DiscountEndDate = vm.DiscountEndDate,
                Stock = vm.Stock,
                Description = vm.Description,
                IsActive = vm.IsActive,
                Slug = vm.Name.ToLower().Replace(" ", "-").Replace("'", "")
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product;
        }

        // Product images upload করে save করে (প্রথমটি primary)
        public async Task AddProductImagesAsync(int productId, IList<IFormFile> images, IImageService imageService)
        {
            bool isFirst = true;
            foreach (var file in images.Where(f => f.Length > 0))
            {
                var url = await imageService.UploadAsync(file, "products");
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url,
                    IsPrimary = isFirst
                });
                isFirst = false;
            }
            await _db.SaveChangesAsync();
        }

        // Product sizes save করে
        public async Task AddProductSizesAsync(int productId, List<ProductSizeEntry> sizes)
        {
            foreach (var s in sizes.Where(s => !string.IsNullOrWhiteSpace(s.SizeName)))
            {
                _db.ProductSizes.Add(new ProductSize
                {
                    ProductId = productId,
                    SizeName = s.SizeName,
                    SizeCode = s.SizeCode,
                    SalesPrice = s.SalesPrice,
                    Stock = s.Stock,
                    IsActive = s.IsActive
                });
            }
            await _db.SaveChangesAsync();
        }

        // ── Edit ──────────────────────────────────────────────────────────────

        // Images ও sizes সহ product খোঁজে
        public async Task<Product?> GetProductWithImagesAndSizesAsync(int id)
        {
            return await _db.Products
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Product এর basic fields update করে
        public async Task UpdateProductAsync(Product product, ProductCreateViewModel vm)
        {
            product.Name = vm.Name;
            product.ProductCode = vm.ProductCode;
            product.CategoryId = vm.CategoryId;
            product.Price = vm.Price;
            product.DiscountPrice = vm.DiscountPrice;
            product.PurchasePrice = vm.PurchasePrice;
            product.ProductVat = vm.ProductVat;
            product.IsDiscountFixed = vm.IsDiscountFixed;
            product.DiscountStartDate = vm.DiscountStartDate;
            product.DiscountEndDate = vm.DiscountEndDate;
            product.Stock = vm.Stock;
            product.Description = vm.Description;
            product.IsActive = vm.IsActive;
            product.Slug = vm.Name.ToLower().Replace(" ", "-").Replace("'", "");
            await _db.SaveChangesAsync();
        }

        // Primary image পরিবর্তন করে
        public async Task UpdatePrimaryImageAsync(Product product, int? primaryImageId)
        {
            if (!primaryImageId.HasValue) return;
            foreach (var img in product.Images)
                img.IsPrimary = (img.Id == primaryImageId.Value);
            await _db.SaveChangesAsync();
        }

        // নির্দিষ্ট images মুছে ফেলে, primary না থাকলে প্রথমটাকে primary করে
        public async Task RemoveImagesAsync(Product product, List<int> removeIds, IImageService imageService)
        {
            if (!removeIds.Any()) return;

            var toRemove = product.Images.Where(i => removeIds.Contains(i.Id)).ToList();
            foreach (var img in toRemove)
            {
                imageService.Delete(img.ImageUrl);
                _db.ProductImages.Remove(img);
            }

            // Primary না থাকলে remaining এর প্রথমটাকে primary করো
            var remaining = product.Images.Where(i => !removeIds.Contains(i.Id)).ToList();
            if (remaining.Any() && !remaining.Any(i => i.IsPrimary))
                remaining.First().IsPrimary = true;

            await _db.SaveChangesAsync();
        }

        // নতুন images যোগ করে (primary না থাকলে প্রথমটাকে primary করে)
        public async Task AddNewImagesAsync(int productId, Product product, IList<IFormFile> images, IImageService imageService)
        {
            bool noPrimary = !product.Images.Any(i => i.IsPrimary);
            bool isFirst = noPrimary;

            foreach (var file in images.Where(f => f.Length > 0))
            {
                var url = await imageService.UploadAsync(file, "products");
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url,
                    IsPrimary = isFirst
                });
                isFirst = false;
            }
            await _db.SaveChangesAsync();
        }

        // Size গুলো update/add/remove করে
        public async Task UpdateSizesAsync(int productId, Product product, List<ProductSizeEntry> sizes)
        {
            foreach (var entry in sizes)
            {
                if (string.IsNullOrWhiteSpace(entry.SizeName)) continue;

                if (entry.Id.HasValue)
                {
                    var existing = product.Sizes.FirstOrDefault(s => s.Id == entry.Id.Value);
                    if (existing == null) continue;

                    if (entry.IsRemove)
                    {
                        _db.ProductSizes.Remove(existing);
                    }
                    else
                    {
                        existing.SizeName = entry.SizeName;
                        existing.SizeCode = entry.SizeCode;
                        existing.SalesPrice = entry.SalesPrice;
                        existing.Stock = entry.Stock;
                        existing.IsActive = entry.IsActive;
                    }
                }
                else if (!entry.IsRemove)
                {
                    _db.ProductSizes.Add(new ProductSize
                    {
                        ProductId = productId,
                        SizeName = entry.SizeName,
                        SizeCode = entry.SizeCode,
                        SalesPrice = entry.SalesPrice,
                        Stock = entry.Stock,
                        IsActive = entry.IsActive
                    });
                }
            }
            await _db.SaveChangesAsync();
        }

        // ── Toggle / Delete ───────────────────────────────────────────────────

        // Id দিয়ে product খোঁজে
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products.FindAsync(id);
        }

        // IsActive toggle করে
        public async Task ToggleActiveAsync(Product product)
        {
            product.IsActive = !product.IsActive;
            await _db.SaveChangesAsync();
        }

        // Product ও সব image মুছে ফেলে
        public async Task DeleteProductAsync(Product product, IImageService imageService)
        {
            var productWithImages = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productWithImages == null) return;

            foreach (var img in productWithImages.Images)
                imageService.Delete(img.ImageUrl);

            _db.Products.Remove(productWithImages);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }
}
