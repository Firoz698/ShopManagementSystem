using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IAdminOrderRepository _orderRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IAdminOrderRepository orderRepo, UserManager<ApplicationUser> userManager)
        {
            _orderRepo = orderRepo;
            _userManager = userManager;
        }

        // GET /Admin/Order
        public async Task<IActionResult> Index(string? status)
        {
            var orders = await _orderRepo.GetFilteredOrdersAsync(status);

            ViewBag.Status = status;
            ViewBag.StatusList = new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            ViewBag.PendingCount = await _orderRepo.GetPendingOrdersCountAsync();

            return View(orders);
        }

        // GET /Admin/Order/ReturnRequest
        public async Task<IActionResult> ReturnRequest(int orderId, int productId)
        {
            var order = await _orderRepo.GetOrderWithUserAsync(orderId);
            var product = await _orderRepo.GetProductByIdAsync(productId);

            if (order == null || product == null) return NotFound();

            ViewBag.Order = order;
            ViewBag.Product = product;
            return View();
        }

        // POST /Admin/Order/ReturnRequest
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnRequest(int orderId, int productId, string reason)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            // আগে return request দেওয়া হয়েছে কিনা চেক
            var alreadyExists = await _orderRepo.HasReturnRequestAsync(orderId, productId, userId);
            if (alreadyExists)
            {
                TempData["Error"] = "এই পণ্যের জন্য ইতিমধ্যে রিটার্ন রিকোয়েস্ট পাঠানো হয়েছে।";
                return RedirectToAction("Index");
            }

            // Order টি এই user এর কিনা verify
            var order = await _orderRepo.GetUserOrderAsync(orderId, userId);
            if (order == null)
            {
                TempData["Error"] = "অর্ডার পাওয়া যায়নি।";
                return RedirectToAction("Index");
            }

            await _orderRepo.AddReturnRequestAsync(orderId, productId, userId, reason);

            TempData["Success"] = "রিটার্ন রিকোয়েস্ট সফলভাবে পাঠানো হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _orderRepo.GetOrderDetailAsync(id);
            if (order == null) return NotFound();

            ViewBag.StatusList = new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            return View(order);
        }

        // POST /Admin/Order/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order != null)
            {
                await _orderRepo.UpdateStatusAsync(order, status);
                TempData["Success"] = $"অর্ডার #{id} স্ট্যাটাস '{status}' করা হয়েছে।";
            }
            return RedirectToAction("Detail", new { id });
        }

        // POST /Admin/Order/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order != null)
                await _orderRepo.DeleteAsync(order);

            TempData["Success"] = "অর্ডার মুছে ফেলা হয়েছে।";
            return RedirectToAction("Index");
        }
    }
}