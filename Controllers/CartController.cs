using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext         _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db          = db;
            _userManager = userManager;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // GET /Cart
        public async Task<IActionResult> Index()
        {
            var items = await _db.Carts
                .Where(c => c.UserId == UserId)
                .Include(c => c.Product).ThenInclude(p => p!.Images)
                .ToListAsync();

            var vm = new CartViewModel
            {
                Items = items.Select(c => new CartItemViewModel
                {
                    CartId    = c.Id,
                    ProductId = c.ProductId,
                    Name      = c.Product!.Name,
                    ImageUrl  = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                    Price    = c.Product.DiscountPrice ?? c.Product.Price,
                    Quantity = c.Quantity,
                    Stock    = c.Product.Stock
                }).ToList()
            };
            return View(vm);
        }

        // POST /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "প্রোডাক্ট পাওয়া যায়নি।";
                return RedirectToAction("Detail", "Product", new { id = productId });
            }

            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == UserId && c.ProductId == productId);
            if (cart == null)
                _db.Carts.Add(new Cart { UserId = UserId, ProductId = productId, Quantity = quantity });
            else
                cart.Quantity = Math.Min(cart.Quantity + quantity, product.Stock);

            await _db.SaveChangesAsync();
            TempData["Success"] = "কার্টে যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var cart = await _db.Carts.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == UserId);
            if (cart != null)
            {
                cart.Quantity = Math.Max(1, Math.Min(quantity, cart.Product!.Stock));
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == UserId);
            if (cart != null) { _db.Carts.Remove(cart); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // GET /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var user  = await _userManager.GetUserAsync(User);
            var items = await _db.Carts
                .Where(c => c.UserId == UserId)
                .Include(c => c.Product).ThenInclude(p => p!.Images)
                .ToListAsync();

            if (!items.Any()) return RedirectToAction("Index");

            var vm = new CheckoutViewModel
            {
                ShippingAddress = user!.Address ?? string.Empty,
                Phone           = user.PhoneNumber ?? string.Empty,
                Cart = new CartViewModel
                {
                    Items = items.Select(c => new CartItemViewModel
                    {
                        CartId    = c.Id,
                        ProductId = c.ProductId,
                        Name      = c.Product!.Name,
                        ImageUrl  = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                 ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                        Price    = c.Product.DiscountPrice ?? c.Product.Price,
                        Quantity = c.Quantity,
                        Stock    = c.Product.Stock
                    }).ToList()
                }
            };
            return View(vm);
        }

        // POST /Cart/PlaceOrder
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
        {
            var items = await _db.Carts
                .Where(c => c.UserId == UserId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!items.Any()) return RedirectToAction("Index");

            // Stock validation
            foreach (var item in items)
            {
                if (item.Product!.Stock < item.Quantity)
                {
                    TempData["Error"] = $"'{item.Product.Name}' এর পর্যাপ্ত স্টক নেই।";
                    return RedirectToAction("Checkout");
                }
            }

            var total = items.Sum(i => (i.Product!.DiscountPrice ?? i.Product.Price) * i.Quantity);

            var order = new Order
            {
                UserId          = UserId,
                ShippingAddress = vm.ShippingAddress,
                Phone           = vm.Phone,
                PaymentMethod   = vm.PaymentMethod,
                Notes           = vm.Notes,
                TotalAmount     = total,
                Status          = "Pending"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in items)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId   = order.Id,
                    ProductId = item.ProductId,
                    Quantity  = item.Quantity,
                    UnitPrice = item.Product!.DiscountPrice ?? item.Product.Price
                });
                item.Product.Stock -= item.Quantity;
            }

            _db.Carts.RemoveRange(items);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"অর্ডার #{order.Id} সফলভাবে দেওয়া হয়েছে!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // GET /Cart/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET /Cart/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == UserId)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }
    }
}
