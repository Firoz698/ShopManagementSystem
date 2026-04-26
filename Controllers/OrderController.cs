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

        // ── POST /Order/SubmitReview ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int productId, int orderId, int rating, string? comment)
        {
            // শুধু Completed অর্ডারেই রিভিউ দেওয়া যাবে
            var order = await _orderRepo.GetCompletedOrderAsync(orderId, UserId);
            if (order == null)
            {
                TempData["Error"] = "শুধুমাত্র সম্পন্ন অর্ডারে রিভিউ দেওয়া যাবে।";
                return RedirectToAction("MyOrders", "Cart");
            }

            // আগে রিভিউ দিয়েছে কিনা চেক
            var alreadyReviewed = await _orderRepo.HasUserReviewedProductAsync(UserId, productId);
            if (!alreadyReviewed)
            {
                await _orderRepo.AddReviewAsync(UserId, productId, rating, comment);
                TempData["Success"] = "রিভিউ সফলভাবে জমা দেওয়া হয়েছে।";
            }
            else
            {
                TempData["Error"] = "আপনি এই পণ্যে আগেই রিভিউ দিয়েছেন।";
            }

            return RedirectToAction("MyOrders", "Cart");
        }

        // ── GET /Order/ReturnRequest/5 ───────────────────────────────────────────
        public async Task<IActionResult> ReturnRequest(int orderId, int productId)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId, UserId);
            if (order == null) return NotFound();

            // শুধু Completed বা Shipped অর্ডারে রিটার্ন করা যাবে
            if (order.Status != "Completed" && order.Status != "Shipped")
            {
                TempData["Error"] = "শুধুমাত্র Shipped বা Completed অর্ডারে রিটার্ন করা যাবে।";
                return RedirectToAction("MyOrders", "Cart");
            }

            // আগে রিটার্ন রিকোয়েস্ট দিয়েছে কিনা
            var alreadyRequested = await _orderRepo.HasReturnRequestAsync(orderId, productId, UserId);
            if (alreadyRequested)
            {
                TempData["Error"] = "এই পণ্যের জন্য আপনি ইতোমধ্যে রিটার্ন রিকোয়েস্ট দিয়েছেন।";
                return RedirectToAction("MyOrders", "Cart");
            }

            var product = order.OrderDetails.FirstOrDefault(od => od.ProductId == productId)?.Product;
            if (product == null) return NotFound();

            ViewBag.Order = order;
            ViewBag.Product = product;
            return View();
        }

        // ── POST /Order/ReturnRequest ────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnRequest(int orderId, int productId, string reason)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId, UserId);
            if (order == null) return NotFound();

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "রিটার্নের কারণ লিখুন।";
                return RedirectToAction("ReturnRequest", new { orderId, productId });
            }

            await _orderRepo.AddReturnRequestAsync(orderId, productId, UserId, reason);
            TempData["Success"] = "রিটার্ন রিকোয়েস্ট সফলভাবে জমা দেওয়া হয়েছে। অ্যাডমিন শীঘ্রই যোগাযোগ করবে।";
            return RedirectToAction("MyOrders", "Cart");
        }

        // ── GET /Order/MyReturns ─────────────────────────────────────────────────
        public async Task<IActionResult> MyReturns()
        {
            var returns = await _orderRepo.GetMyReturnsAsync(UserId);
            return View(returns);
        }
    }
}