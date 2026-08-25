using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Interfaces;
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
        private readonly INotificationService _notificationService;
        private readonly IPaymentMethodRepository _paymentMethodRepo;

        public CartController(
            ICartRepository cartRepo,
            UserManager<ApplicationUser> userManager,
            ISslCommerzService sslService,
            INotificationService notificationService,
            IPaymentMethodRepository paymentMethodRepo)
        {
            _cartRepo = cartRepo;
            _userManager = userManager;
            _sslService = sslService;
            _notificationService = notificationService;
            _paymentMethodRepo = paymentMethodRepo;
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

        // ── POST /Cart/Add ───────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, int? productSizeId = null)
        {
            var product = await _cartRepo.GetActiveProductWithSizesAsync(productId);

            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "প্রোডাক্ট পাওয়া যায়নি।";
                return RedirectToAction("Index");
            }

            if (productSizeId.HasValue && productSizeId.Value <= 0)
                productSizeId = null;

            if (productSizeId == null && product.Sizes.Any(s => s.IsActive && s.Stock > 0))
                productSizeId = product.Sizes.First(s => s.IsActive && s.Stock > 0).Id;

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

            if (productSizeId.HasValue && productSizeId.Value <= 0)
                productSizeId = null;

            if (productSizeId == null && product.Sizes.Any(s => s.IsActive && s.Stock > 0))
                productSizeId = product.Sizes.First(s => s.IsActive && s.Stock > 0).Id;

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

            decimal total = items.Sum(i =>
                (i.ProductSize?.SalesPrice ?? i.Product!.DiscountPrice ?? i.Product!.Price) * i.Quantity);

            decimal deliveryCharge = vm.DeliveryZone == "ঢাকার বাইরে" ? 120 : 60;
            total += deliveryCharge;

            var currentUser = await _userManager.GetUserAsync(User);
            var isOnlinePayment = vm.PaymentMethod == "Online Payment";

            // ══════════════════════════════════════════════════════════════════
            // ── Online Payment: আগে গেটওয়ে ইনিশিয়েট, stock/cart/notify পরে ──
            // ══════════════════════════════════════════════════════════════════
            if (isOnlinePayment)
            {
                // Pending Payment status এ order তৈরি — stock/cart এখনো touch হয়নি
                var pendingOrder = await _cartRepo.CreateOrderAsync(UserId, vm, total, status: "Pending Payment");
                await _cartRepo.AddOrderDetailsAsync(pendingOrder.Id, items);

                var tranId = $"TXN-{pendingOrder.Id}-{DateTime.Now.Ticks}";
                await _cartRepo.CreatePaymentTransactionAsync(pendingOrder.Id, tranId, total);

                var gatewayUrl = await _sslService.InitiatePaymentAsync(new SslPaymentRequest
                {
                    TransactionId = tranId,
                    Amount = total,
                    CustomerName = currentUser!.FullName,
                    CustomerEmail = currentUser.Email ?? "",
                    CustomerPhone = vm.Phone,
                    ShippingAddress = vm.ShippingAddress,
                    ProductName = $"ShopMS Order #{pendingOrder.Id}"
                });

                if (!string.IsNullOrEmpty(gatewayUrl))
                {
                    // ✅ সফল — গেটওয়েতে পাঠাও। Stock deduct/cart clear/notification
                    // এখনো হয়নি — সেগুলো হবে PaymentSuccess/IPN এ, পেমেন্ট কনফার্ম হওয়ার পরে
                    return Redirect(gatewayUrl);
                }

                // ❌ গেটওয়ে ব্যর্থ — stock/cart কিছুই কমেনি, order কে স্পষ্টভাবে fail মার্ক করো
                await _cartRepo.UpdateOrderStatusAsync(pendingOrder, "Payment Failed");
                TempData["Error"] = "পেমেন্ট গেটওয়ে সংযোগ ব্যর্থ হয়েছে। আবার চেষ্টা করুন অথবা ক্যাশ অন ডেলিভারি বেছে নিন।";
                return RedirectToAction("Checkout");
            }

            // ══════════════════════════════════════════════════════════════════
            // ── Cash on Delivery: আগের মতোই সব সাথে সাথে কনফার্ম হবে ──
            // ══════════════════════════════════════════════════════════════════
            var order = await _cartRepo.CreateOrderAsync(UserId, vm, total, status: "Pending");
            await _cartRepo.AddOrderDetailsAsync(order.Id, items);
            await _cartRepo.DeductStockAsync(items);
            await _cartRepo.ClearCartAsync(items);

            try
            {
                await _notificationService.SendNewOrderNotificationAsync(order.Id, currentUser!.FullName, total);
            }
            catch
            {
                // notification ব্যর্থ হলেও order flow থামবে না
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

                // ✅ পেমেন্ট কনফার্ম হওয়ার পরেই stock deduct, cart clear, notification
                var order = payment.Order!;
                var items = await _cartRepo.GetCartItemsAsync(order.UserId);

                if (items.Any())
                {
                    await _cartRepo.DeductStockAsync(items);
                    await _cartRepo.ClearCartAsync(items);
                }

                try
                {
                    var buyer = await _userManager.FindByIdAsync(order.UserId);
                    await _notificationService.SendNewOrderNotificationAsync(order.Id, buyer!.FullName, payment.Amount);
                }
                catch
                {
                    // notification ব্যর্থ হলেও payment flow থামবে না
                }

                TempData["Success"] = $"পেমেন্ট সফল! অর্ডার #{payment.OrderId} কনফার্ম।";
                return RedirectToAction("OrderConfirmation", new { id = payment.OrderId });
            }

            await _cartRepo.UpdatePaymentStatusAsync(payment, "Failed");
            await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Payment Failed");

            TempData["Error"] = "পেমেন্ট যাচাই করা যায়নি।";
            return RedirectToAction("PaymentFail", new { tran_id });
        }

        // ── GET /Cart/PaymentFail ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentFail(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
            {
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Failed");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Payment Failed");
            }

            TempData["Error"] = "পেমেন্ট ব্যর্থ হয়েছে।";
            return View("PaymentFail", payment);
        }

        // ── GET /Cart/PaymentCancel ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
            {
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Cancelled");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Cancelled");
            }

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

                // ✅ IPN দিয়ে সার্ভার-টু-সার্ভার confirm হলেও একইভাবে stock/cart handle করো
                var order = payment.Order!;
                var items = await _cartRepo.GetCartItemsAsync(order.UserId);

                if (items.Any())
                {
                    await _cartRepo.DeductStockAsync(items);
                    await _cartRepo.ClearCartAsync(items);
                }

                try
                {
                    var buyer = await _userManager.FindByIdAsync(order.UserId);
                    await _notificationService.SendNewOrderNotificationAsync(order.Id, buyer!.FullName, payment.Amount);
                }
                catch { }
            }

            return Ok();
        }
    }
}