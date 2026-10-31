using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class DeliveryController : Controller
    {
        private readonly IAdminOrderRepository _orderRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionChecker _permissionChecker;

        public DeliveryController(
            IAdminOrderRepository orderRepo,
            UserManager<ApplicationUser> userManager,
            IPermissionChecker permissionChecker)
        {
            _orderRepo = orderRepo;
            _userManager = userManager;
            _permissionChecker = permissionChecker;
        }

        // GET: /Admin/Delivery
        public async Task<IActionResult> Index(string? status, string? search, string? filterType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            string? assignedEmployeeId = null;
            if (filterType == "my")
            {
                assignedEmployeeId = user.Id;
            }

            var orders = await _orderRepo.GetDeliveryOrdersAsync(status, search, assignedEmployeeId);
            var allOrders = await _orderRepo.GetDeliveryOrdersAsync(null, null, null);

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.FilterType = filterType;
            ViewBag.DeliveryStaffList = await _orderRepo.GetDeliveryStaffListAsync();

            // Quick Stats
            ViewBag.TotalOrders = allOrders.Count;
            ViewBag.PendingAssignmentCount = allOrders.Count(o => string.IsNullOrEmpty(o.AssignedEmployeeId) && o.Status != "Completed" && o.Status != "Cancelled");
            ViewBag.OutForDeliveryCount = allOrders.Count(o => o.Status == "Out for Delivery");
            ViewBag.CompletedCount = allOrders.Count(o => o.Status == "Completed");
            ViewBag.MyAssignedCount = allOrders.Count(o => o.AssignedEmployeeId == user.Id && o.Status != "Completed" && o.Status != "Cancelled");

            return View(orders);
        }

        // GET: /Admin/Delivery/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _orderRepo.GetDeliveryOrderAsync(id);
            if (order == null) return NotFound();

            ViewBag.DeliveryStaffList = await _orderRepo.GetDeliveryStaffListAsync();
            ViewBag.StatusList = new[] { "Pending", "Processing", "Shipped", "Out for Delivery", "Completed", "Cancelled" };

            return View(order);
        }

        // POST: /Admin/Delivery/AssignRider
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRider(int orderId, string employeeId, DateTime? estimatedDate, string? notes)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Delivery");
            if (!perm.CanEdit)
            {
                TempData["Error"] = "You do not have permission to assign delivery staff.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                TempData["Error"] = "Please select an employee/delivery rider.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var updatedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Admin";

            await _orderRepo.AssignRiderAsync(orderId, employeeId, estimatedDate, notes, updatedBy);
            TempData["Success"] = $"Delivery rider successfully assigned to Order #{orderId}!";

            return RedirectToAction("Detail", new { id = orderId });
        }

        // POST: /Admin/Delivery/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status, string? location, string? notes)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Delivery");
            if (!perm.CanEdit)
            {
                TempData["Error"] = "You do not have permission to update delivery status.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var updatedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Admin";

            await _orderRepo.UpdateDeliveryStatusAsync(orderId, status, location, notes, updatedBy);
            TempData["Success"] = $"Order #{orderId} delivery status updated to '{status}'.";

            return RedirectToAction("Detail", new { id = orderId });
        }

        // POST: /Admin/Delivery/VerifyOtp
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(int orderId, string otp, string? notes)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Delivery");
            if (!perm.CanEdit)
            {
                TempData["Error"] = "You do not have permission to complete deliveries.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                TempData["Error"] = "Please enter the 6-digit Delivery OTP provided by customer.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var updatedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Delivery Hero";

            var result = await _orderRepo.VerifyOtpAndCompleteDeliveryAsync(orderId, otp, notes, updatedBy);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Detail", new { id = orderId });
        }

        // POST: /Admin/Delivery/AdminOverrideDeliver
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminOverrideDeliver(int orderId, string? notes)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Delivery");
            if (!perm.CanEdit)
            {
                TempData["Error"] = "Permission denied.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var updatedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Admin";

            await _orderRepo.AdminOverrideCompleteDeliveryAsync(orderId, notes, updatedBy);
            TempData["Success"] = $"Order #{orderId} marked as Delivered via Admin Override.";

            return RedirectToAction("Detail", new { id = orderId });
        }

        // POST: /Admin/Delivery/AddLog
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLog(int orderId, string status, string title, string? description, string? location)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Delivery");
            if (!perm.CanEdit)
            {
                TempData["Error"] = "Permission denied.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Please provide an event title.";
                return RedirectToAction("Detail", new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var updatedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Staff";

            await _orderRepo.AddTrackingLogAsync(orderId, status, title, description, location, updatedBy);
            TempData["Success"] = "Tracking log event added successfully.";

            return RedirectToAction("Detail", new { id = orderId });
        }
    }
}

