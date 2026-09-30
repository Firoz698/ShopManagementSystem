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

        // GET /Cart/GetCount
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetCount()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Json(new { count = 0 });

            var count = await _cartRepo.GetCartCountAsync(UserId);
            return Json(new { count });
        }

        // GET /Cart
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

        // POST /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, int? productSizeId = null)
        {
            var product = await _cartRepo.GetActiveProductWithSizesAsync(productId);

            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "Product not found.";
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
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Detail", "Product", new { id = productId });
            }

            await _cartRepo.AddToCartAsync(UserId, productId, productSizeId, quantity);
            TempData["Success"] = "Product added to cart.";
            return RedirectToAction("Index");
        }

        // POST /Cart/AddAjax
        [HttpPost, IgnoreAntiforgeryToken, AllowAnonymous]
        public async Task<IActionResult> AddAjax(int productId, int quantity = 1, int? productSizeId = null)
        {
            try
            {
                if (User.Identity?.IsAuthenticated != true)
                    return Json(new { success = false, message = "Please login first to add items to your cart.", requireLogin = true });

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Please login first to add items to your cart.", requireLogin = true });

                var product = await _cartRepo.GetActiveProductWithSizesAsync(productId);

                if (product == null || !product.IsActive)
                    return Json(new { success = false, message = "Product not found or unavailable." });

                if (productSizeId.HasValue && productSizeId.Value <= 0)
                    productSizeId = null;

                if (productSizeId == null && product.Sizes != null && product.Sizes.Any(s => s.IsActive && s.Stock > 0))
                    productSizeId = product.Sizes.First(s => s.IsActive && s.Stock > 0).Id;

                int availableStock = productSizeId.HasValue
                    ? product.Sizes?.FirstOrDefault(s => s.Id == productSizeId.Value)?.Stock ?? 0
                    : product.Stock;

                if (availableStock < quantity)
                    return Json(new { success = false, message = "Not enough stock available." });

                await _cartRepo.AddToCartAsync(userId, productId, productSizeId, quantity);

                var cartCount = await _cartRepo.GetCartCountAsync(userId);
                return Json(new { success = true, cartCount, message = "Product added to cart." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding to cart: " + ex.Message });
            }
        }

        // POST /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var cart = await _cartRepo.GetCartByIdAsync(cartId, UserId);
            if (cart != null)
                await _cartRepo.UpdateCartQuantityAsync(cart, quantity);

            return RedirectToAction("Index");
        }

        // POST /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _cartRepo.GetCartByIdAsync(cartId, UserId);
            if (cart != null)
                await _cartRepo.RemoveFromCartAsync(cart);

            return RedirectToAction("Index");
        }

        // GET /Cart/Checkout
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

        // POST /Cart/PlaceOrder
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
                    TempData["Error"] = $"'{item.Product!.Name}' does not have enough stock.";
                    return RedirectToAction("Checkout");
                }
            }

            decimal total = items.Sum(i =>
                (i.ProductSize?.SalesPrice ?? i.Product!.DiscountPrice ?? i.Product!.Price) * i.Quantity);

            decimal deliveryCharge = (vm.DeliveryZone == "Outside Dhaka") ? 120 : 60;
            total += deliveryCharge;

            var currentUser = await _userManager.GetUserAsync(User);
            var isOnlinePayment = vm.PaymentMethod == "Online Payment";

            // Online Payment
            if (isOnlinePayment)
            {
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
                    return Redirect(gatewayUrl);
                }

                await _cartRepo.UpdateOrderStatusAsync(pendingOrder, "Payment Failed");
                TempData["Error"] = "Payment gateway connection failed. Please try again or select Cash on Delivery.";
                return RedirectToAction("Checkout");
            }

            // Cash on Delivery
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
                // Proceed even if notification fails
            }

            TempData["Success"] = $"Order #{order.Id} has been placed successfully!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // GET /Cart/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _cartRepo.GetOrderConfirmationAsync(id, UserId);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET /Cart/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _cartRepo.GetMyOrdersAsync(UserId);
            return View(orders);
        }

        // GET /Cart/PaymentSuccess
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
                }

                TempData["Success"] = $"Payment successful! Order #{payment.OrderId} confirmed.";
                return RedirectToAction("OrderConfirmation", new { id = payment.OrderId });
            }

            await _cartRepo.UpdatePaymentStatusAsync(payment, "Failed");
            await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Payment Failed");

            TempData["Error"] = "Payment could not be verified.";
            return RedirectToAction("PaymentFail", new { tran_id });
        }

        // GET /Cart/PaymentFail
        [HttpGet]
        public async Task<IActionResult> PaymentFail(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
            {
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Failed");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Payment Failed");
            }

            TempData["Error"] = "Payment failed.";
            return View("PaymentFail", payment);
        }

        // GET /Cart/PaymentCancel
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(string tran_id)
        {
            var payment = await _cartRepo.GetPaymentByTranIdAsync(tran_id);
            if (payment != null)
            {
                await _cartRepo.UpdatePaymentStatusAsync(payment, "Cancelled");
                await _cartRepo.UpdateOrderStatusAsync(payment.Order, "Cancelled");
            }

            TempData["Error"] = "Payment has been cancelled.";
            return View("PaymentCancel", payment);
        }

        // POST /Cart/IPN
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