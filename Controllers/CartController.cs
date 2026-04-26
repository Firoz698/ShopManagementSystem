using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISslCommerzService _sslService;

        public CartController(
            ICartRepository cartRepo,
            UserManager<ApplicationUser> userManager,
            ISslCommerzService sslService)
        {
            _cartRepo = cartRepo;
            _userManager = userManager;
            _sslService = sslService;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // ── GET /Cart ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var items = await _cartRepo.GetCartItemsAsync(UserId);

            var vm = new CartViewModel
            {
                Items = items.Select(c => new CartItemViewModel
                {
                    CartId = c.Id,
                    ProductId = c.ProductId,
                    ProductSizeId = c.ProductSizeId,
                    SizeName = c.ProductSize?.SizeName,
                    Name = c.Product!.Name,
                    ImageUrl = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                      ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                    Price = c.ProductSize?.SalesPrice ?? c.Product.DiscountPrice ?? c.Product.Price,
                    Quantity = c.Quantity,
                    Stock = c.ProductSize?.Stock ?? c.Product.Stock
                }).ToList()
            };
            return View(vm);
        }

        // ── POST /Cart/Add (Normal form POST) ────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, int? productSizeId = null)
        {
            var product = await _cartRepo.GetActiveProductWithSizesAsync(productId);

            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "প্রোডাক্ট পাওয়া যায়নি।";
                return RedirectToAction("Index");
            }

            // 0 বা invalid sizeId কে null করুন
            if (productSizeId.HasValue && productSizeId.Value <= 0)
                productSizeId = null;

            // Size আছে কিন্তু select করেনি — প্রথম available size নিন
            if (productSizeId == null && product.Sizes.Any(s => s.IsActive && s.Stock > 0))
                productSizeId = product.Sizes.First(s => s.IsActive && s.Stock > 0).Id;

            // Stock validation
            int availableStock = productSizeId.HasValue
                ? product.Sizes.FirstOrDefault(s => s.Id == productSizeId.Value)?.Stock ?? 0
                : product.Stock;

            if (availableStock < quantity)
            {
                TempData["Error"] = "পর্যাপ্ত স্টক নেই।";
                return RedirectToAction("Detail", "Product", new { id = productId });
            }

            await _cartRepo.AddToCartAsync(UserId, productId, productSizeId, quantity);
            TempData["Success"] = "কার্টে যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // ── POST /Cart/AddAjax ───────────────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddAjax(int productId, int quantity = 1, int? productSizeId = null)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false, message = "লগইন করুন" });

            var product = await _cartRepo.GetActiveProductWithSizesAsync(productId);

            if (product == null || !product.IsActive)
                return Json(new { success = false, message = "প্রোডাক্ট পাওয়া যায়নি।" });

            // 0 বা invalid sizeId কে null করুন
            if (productSizeId.HasValue && productSizeId.Value <= 0)
                productSizeId = null;

            // Size আছে কিন্তু select করেনি — প্রথম available size নিন
            if (productSizeId == null && product.Sizes.Any(s => s.IsActive && s.Stock > 0))
                productSizeId = product.Sizes.First(s => s.IsActive && s.Stock > 0).Id;

            // Stock check
            int availableStock = productSizeId.HasValue
                ? product.Sizes.FirstOrDefault(s => s.Id == productSizeId.Value)?.Stock ?? 0
                : product.Stock;

            if (availableStock < quantity)
                return Json(new { success = false, message = "পর্যাপ্ত স্টক নেই।" });

            await _cartRepo.AddToCartAsync(UserId, productId, productSizeId, quantity);

            var cartCount = await _cartRepo.GetCartCountAsync(UserId);
            return Json(new { success = true, cartCount, message = "কার্টে যোগ হয়েছে।" });
        }

        // ── POST /Cart/UpdateQuantity ────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var cart = await _cartRepo.GetCartByIdAsync(cartId, UserId);
            if (cart != null)
                await _cartRepo.UpdateCartQuantityAsync(cart, quantity);

            return RedirectToAction("Index");
        }

        // ── POST /Cart/Remove ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _cartRepo.GetCartByIdAsync(cartId, UserId);
            if (cart != null)
                await _cartRepo.RemoveFromCartAsync(cart);

            return RedirectToAction("Index");
        }

        // ── GET /Cart/Checkout ───────────────────────────────────────────────────
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = await _cartRepo.GetCartItemsAsync(UserId);

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
                        ProductSizeId = c.ProductSizeId,
                        SizeName = c.ProductSize?.SizeName,
                        Name = c.Product!.Name,
                        ImageUrl = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                          ?? c.Product.Images.FirstOrDefault()?.ImageUrl,
                        Price = c.ProductSize?.SalesPrice ?? c.Product.DiscountPrice ?? c.Product.Price,
                        Quantity = c.Quantity,
                        Stock = c.ProductSize?.Stock ?? c.Product.Stock
                    }).ToList()
                }
            };
            return View(vm);
        }

        // ── POST /Cart/PlaceOrder ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
        {
            var items = await _cartRepo.GetCartItemsAsync(UserId);
            if (!items.Any()) return RedirectToAction("Index");

            // Stock validation
            foreach (var item in items)
            {
                int stock = item.ProductSize?.Stock ?? item.Product!.Stock;
                if (stock < item.Quantity)
                {
                    TempData["Error"] = $"'{item.Product!.Name}' এর পর্যাপ্ত স্টক নেই।";
                    return RedirectToAction("Checkout");
                }
            }

            // Total calculate
            decimal total = items.Sum(i =>
                (i.ProductSize?.SalesPrice ?? i.Product!.DiscountPrice ?? i.Product!.Price) * i.Quantity);

            decimal deliveryCharge = vm.DeliveryZone == "ঢাকার বাইরে" ? 120 : 60;
            total += deliveryCharge;

            // Order তৈরি
            var order = await _cartRepo.CreateOrderAsync(UserId, vm, total);
            await _cartRepo.AddOrderDetailsAsync(order.Id, items);
            await _cartRepo.DeductStockAsync(items);
            await _cartRepo.ClearCartAsync(items);

            // Online Payment
            if (vm.PaymentMethod == "Online Payment")
            {
                var user = await _userManager.GetUserAsync(User);
                var tranId = $"TXN-{order.Id}-{DateTime.Now.Ticks}";

                await _cartRepo.CreatePaymentTransactionAsync(order.Id, tranId, total);

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

                TempData["Error"] = "পেমেন্ট গেটওয়ে সংযোগ ব্যর্থ। ক্যাশ অন ডেলিভারিতে অর্ডার নেওয়া হয়েছে।";
            }

            TempData["Success"] = $"অর্ডার #{order.Id} সফলভাবে দেওয়া হয়েছে!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // ── GET /Cart/OrderConfirmation/5 ────────────────────────────────────────
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _cartRepo.GetOrderConfirmationAsync(id, UserId);
            if (order == null) return NotFound();
            return View(order);
        }

        // ── GET /Cart/MyOrders ───────────────────────────────────────────────────
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _cartRepo.GetMyOrdersAsync(UserId);
            return View(orders);
        }

        // ── GET /Cart/PaymentSuccess ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(
            string tran_id, string val_id, string amount, string card_type, string status)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
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
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Success", val_id, card_type ?? "Online");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Processing");

                TempData["Success"] = $"পেমেন্ট সফল! অর্ডার #{payment.OrderId} কনফার্ম।";
                return RedirectToAction("OrderConfirmation", new { id = payment.OrderId });
            }

            TempData["Error"] = "পেমেন্ট যাচাই করা যায়নি।";
            return RedirectToAction("PaymentFail", new { tran_id });
        }

        // ── GET /Cart/PaymentFail ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentFail(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Failed");

            TempData["Error"] = "পেমেন্ট ব্যর্থ হয়েছে।";
            return View("PaymentFail", payment);
        }

        // ── GET /Cart/PaymentCancel ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Cancelled");

            TempData["Error"] = "পেমেন্ট বাতিল করা হয়েছে।";
            return View("PaymentCancel", payment);
        }

        // ── POST /Cart/IPN ───────────────────────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> IPN([FromForm] SslIpnResponse ipn)
        {
            if (ipn.tran_id == null) return Ok();

            var isValid = await _sslService.ValidateIpnAsync(ipn);
            if (!isValid) return Ok();

            var payment = await _cartRepo.GetPaymentByTranIdAsync(ipn.tran_id);
            if (payment == null) return Ok();

            if (ipn.status == "VALID" && payment.Status != "Success")
            {
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Success", ipn.val_id ?? "", ipn.card_type ?? "Online");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Processing");
            }

            return Ok();
        }
    }
}