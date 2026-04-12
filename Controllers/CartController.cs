using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISslCommerzService _sslService;

        public CartController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ISslCommerzService sslService)
        {
            _db = db;
            _userManager = userManager;
            _sslService = sslService;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // ── GET /Cart ────────────────────────────────────────────────────────────
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
                    CartId = c.Id,
                    ProductId = c.ProductId,
                    Name = c.Product!.Name,
                    ImageUrl = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                    Price = c.Product.DiscountPrice ?? c.Product.Price,
                    Quantity = c.Quantity,
                    Stock = c.Product.Stock
                }).ToList()
            };
            return View(vm);
        }

        // ── POST /Cart/Add ───────────────────────────────────────────────────────
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

        // ── POST /Cart/UpdateQuantity ────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var cart = await _db.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == UserId);

            if (cart != null)
            {
                cart.Quantity = Math.Max(1, Math.Min(quantity, cart.Product!.Stock));
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // ── POST /Cart/Remove ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == UserId);
            if (cart != null) { _db.Carts.Remove(cart); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        // ── GET /Cart/Checkout ───────────────────────────────────────────────────
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = await _db.Carts
                .Where(c => c.UserId == UserId)
                .Include(c => c.Product).ThenInclude(p => p!.Images)
                .ToListAsync();

            if (!items.Any()) return RedirectToAction("Index");

            var vm = new CheckoutViewModel
            {
                ShippingAddress = user!.Address ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                Cart = new CartViewModel
                {
                    Items = items.Select(c => new CartItemViewModel
                    {
                        CartId = c.Id,
                        ProductId = c.ProductId,
                        Name = c.Product!.Name,
                        ImageUrl = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                 ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                        Price = c.Product.DiscountPrice ?? c.Product.Price,
                        Quantity = c.Quantity,
                        Stock = c.Product.Stock
                    }).ToList()
                }
            };
            return View(vm);
        }

        // ── POST /Cart/PlaceOrder ────────────────────────────────────────────────
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
                UserId = UserId,
                ShippingAddress = vm.ShippingAddress,
                Phone = vm.Phone,
                PaymentMethod = vm.PaymentMethod,
                Notes = vm.Notes,
                TotalAmount = total,
                Status = "Pending"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in items)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.DiscountPrice ?? item.Product.Price
                });
                item.Product.Stock -= item.Quantity;
            }

            _db.Carts.RemoveRange(items);
            await _db.SaveChangesAsync();

            // ── Online Payment হলে SSLCommerz-এ Redirect করুন ──────────────────
            if (vm.PaymentMethod == "Online Payment")
            {
                var user = await _userManager.GetUserAsync(User);
                var tranId = $"TXN-{order.Id}-{DateTime.Now.Ticks}";

                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    OrderId = order.Id,
                    TransactionId = tranId,
                    Amount = total,
                    Status = "Pending"
                });
                await _db.SaveChangesAsync();

                var gatewayUrl = await _sslService.InitiatePaymentAsync(new SslPaymentRequest
                {
                    TransactionId = tranId,
                    Amount = total,
                    CustomerName = user!.FullName,
                    CustomerEmail = user.Email ?? "",
                    CustomerPhone = vm.Phone,
                    ShippingAddress = vm.ShippingAddress,
                    ProductName = $"ShopMS Order #{order.Id}"
                });

                if (!string.IsNullOrEmpty(gatewayUrl))
                    return Redirect(gatewayUrl);

                TempData["Error"] = "পেমেন্ট গেটওয়ে সংযোগ ব্যর্থ হয়েছে। ক্যাশ অন ডেলিভারিতে অর্ডার নেওয়া হয়েছে।";
            }

            TempData["Success"] = $"অর্ডার #{order.Id} সফলভাবে দেওয়া হয়েছে!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // ── GET /Cart/OrderConfirmation/5 ────────────────────────────────────────
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);
            if (order == null) return NotFound();
            return View(order);
        }

        // ── GET /Cart/MyOrders ───────────────────────────────────────────────────
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == UserId)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // ── GET /Cart/PaymentSuccess ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(
            string tran_id, string val_id, string amount,
            string card_type, string status)
        {
            var payment = await _db.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == tran_id);

            if (payment == null) return NotFound();

            var isValid = await _sslService.ValidateIpnAsync(new SslIpnResponse
            {
                tran_id = tran_id,
                val_id = val_id,
                status = status,
                amount = amount
            });

            if (isValid && status == "VALID")
            {
                payment.Status = "Success";
                payment.ValidationId = val_id;
                payment.PaymentMethod = card_type ?? "Online";
                payment.UpdatedAt = DateTime.Now;

                if (payment.Order != null)
                    payment.Order.Status = "Processing";

                await _db.SaveChangesAsync();

                TempData["Success"] = $"পেমেন্ট সফল হয়েছে! অর্ডার #{payment.OrderId} কনফার্ম করা হয়েছে।";
                return RedirectToAction("OrderConfirmation", new { id = payment.OrderId });
            }

            TempData["Error"] = "পেমেন্ট যাচাই করা যায়নি। আমাদের সাথে যোগাযোগ করুন।";
            return RedirectToAction("PaymentFail", new { tran_id });
        }

        // ── GET /Cart/PaymentFail ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentFail(string tran_id)
        {
            var payment = await _db.PaymentTransactions
                .FirstOrDefaultAsync(p => p.TransactionId == tran_id);

            if (payment != null)
            {
                payment.Status = "Failed";
                payment.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }

            TempData["Error"] = "পেমেন্ট ব্যর্থ হয়েছে। আবার চেষ্টা করুন।";
            return View("PaymentFail", payment);
        }

        // ── GET /Cart/PaymentCancel ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(string tran_id)
        {
            var payment = await _db.PaymentTransactions
                .FirstOrDefaultAsync(p => p.TransactionId == tran_id);

            if (payment != null)
            {
                payment.Status = "Cancelled";
                payment.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }

            TempData["Error"] = "পেমেন্ট বাতিল করা হয়েছে।";
            return View("PaymentCancel", payment);
        }

        // ── POST /Cart/IPN  (SSLCommerz এই URL-এ POST করবে) ─────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> IPN([FromForm] SslIpnResponse ipn)
        {
            if (ipn.tran_id == null) return Ok();

            var isValid = await _sslService.ValidateIpnAsync(ipn);
            if (!isValid) return Ok();

            var payment = await _db.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == ipn.tran_id);

            if (payment == null) return Ok();

            if (ipn.status == "VALID" && payment.Status != "Success")
            {
                payment.Status = "Success";
                payment.ValidationId = ipn.val_id ?? "";
                payment.PaymentMethod = ipn.card_type ?? "Online";
                payment.UpdatedAt = DateTime.Now;

                if (payment.Order != null)
                    payment.Order.Status = "Processing";

                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}