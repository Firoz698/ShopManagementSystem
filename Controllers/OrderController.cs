using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderRepository orderRepo, UserManager<ApplicationUser> userManager)
        {
            _orderRepo = orderRepo;
            _userManager = userManager;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // POST /Order/SubmitReview
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int productId, int orderId, int rating, string? comment)
        {
            var order = await _orderRepo.GetCompletedOrderAsync(orderId, UserId);
            if (order == null)
            {
                TempData["Error"] = "Reviews can only be submitted for completed orders.";
                return RedirectToAction("MyOrders", "Cart");
            }

            var alreadyReviewed = await _orderRepo.HasUserReviewedProductAsync(UserId, productId);
            if (!alreadyReviewed)
            {
                await _orderRepo.AddReviewAsync(UserId, productId, rating, comment);
                TempData["Success"] = "Review submitted successfully.";
            }
            else
            {
                TempData["Error"] = "You have already reviewed this product.";
            }

            return RedirectToAction("MyOrders", "Cart");
        }

        // GET /Order/ReturnRequest/5
        public async Task<IActionResult> ReturnRequest(int orderId, int productId)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId, UserId);
            if (order == null) return NotFound();

            if (order.Status != "Completed" && order.Status != "Shipped")
            {
                TempData["Error"] = "Return requests can only be placed for shipped or completed orders.";
                return RedirectToAction("MyOrders", "Cart");
            }

            var alreadyRequested = await _orderRepo.HasReturnRequestAsync(orderId, productId, UserId);
            if (alreadyRequested)
            {
                TempData["Error"] = "You have already submitted a return request for this product.";
                return RedirectToAction("MyOrders", "Cart");
            }

            var product = order.OrderDetails.FirstOrDefault(od => od.ProductId == productId)?.Product;
            if (product == null) return NotFound();

            ViewBag.Order = order;
            ViewBag.Product = product;
            return View();
        }

        // POST /Order/ReturnRequest
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnRequest(int orderId, int productId, string reason)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId, UserId);
            if (order == null) return NotFound();

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Please provide a reason for the return.";
                return RedirectToAction("ReturnRequest", new { orderId, productId });
            }

            await _orderRepo.AddReturnRequestAsync(orderId, productId, UserId, reason);
            TempData["Success"] = "Return request submitted successfully. Our team will contact you soon.";
            return RedirectToAction("MyOrders", "Cart");
        }

        // GET /Order/MyReturns
        public async Task<IActionResult> MyReturns()
        {
            var returns = await _orderRepo.GetMyReturnsAsync(UserId);
            return View(returns);
        }

        // GET /Order/CancelOrder/5
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _orderRepo.GetCancellableOrderAsync(orderId, UserId);
            if (order == null)
            {
                TempData["Error"] = "This order cannot be cancelled (it may have already been Shipped, Completed, or Cancelled).";
                return RedirectToAction("MyOrders", "Cart");
            }

            ViewBag.Order = order;
            return View();
        }

        // POST /Order/CancelOrder
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId, string? reason)
        {
            var order = await _orderRepo.GetCancellableOrderAsync(orderId, UserId);
            if (order == null)
            {
                TempData["Error"] = "This order cannot be cancelled.";
                return RedirectToAction("MyOrders", "Cart");
            }

            await _orderRepo.CancelOrderAsync(order, reason);
            TempData["Success"] = $"Order #{order.Id} has been cancelled.";
            return RedirectToAction("MyOrders", "Cart");
        }

        // GET /Order/Track
        [AllowAnonymous]
        public async Task<IActionResult> Track(int? orderId, string? phone, string? trackingNumber)
        {
            Order? order = null;
            var currentUserId = _userManager.GetUserId(User);

            if (!string.IsNullOrWhiteSpace(trackingNumber))
            {
                order = await _orderRepo.TrackOrderByTrackingNumberAsync(trackingNumber.Trim());
            }
            else if (orderId.HasValue && !string.IsNullOrWhiteSpace(phone))
            {
                order = await _orderRepo.TrackOrderAsync(orderId.Value, phone.Trim());
            }
            else if (orderId.HasValue && !string.IsNullOrEmpty(currentUserId))
            {
                var userOrder = await _orderRepo.TrackOrderByIdAsync(orderId.Value);
                if (userOrder != null && (userOrder.UserId == currentUserId || User.IsInRole("Admin")))
                {
                    order = userOrder;
                }
            }

            ViewBag.SearchedOrderId = orderId;
            ViewBag.SearchedPhone = phone;
            ViewBag.SearchedTrackingNumber = trackingNumber;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                ViewBag.RecentOrders = await _orderRepo.GetUserRecentOrdersAsync(currentUserId, 5);
            }

            return View(order);
        }
    }
}