using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class UserController : Controller
    {
        private readonly IAdminUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public UserController(IAdminUserRepository userRepo, IWebHostEnvironment env)
        {
            _userRepo = userRepo;
            _env = env;
        }

        // GET /Admin/User
        public async Task<IActionResult> Index()
        {
            var users = await _userRepo.GetAllUsersAsync();
            ViewBag.UserRoles = await _userRepo.GetUserRolesAsync(users);
            return View(users);
        }

        // GET /Admin/User/Detail/id
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Orders = await _userRepo.GetUserOrdersAsync(id);
            ViewBag.Roles = await _userRepo.GetRolesAsync(user);
            return View(user);
        }

        // POST /Admin/User/Delete/id
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user != null)
            {
                await _userRepo.RemoveUserRelatedDataAsync(id);
                await _userRepo.DeleteUserAsync(user);
                TempData["Success"] = "User deleted successfully.";
            }
            return RedirectToAction("Index");
        }

        // POST /Admin/User/ToggleRole/id
        [HttpPost]
        public async Task<IActionResult> ToggleRole(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userRepo.GetRolesAsync(user);
            await _userRepo.ToggleRoleAsync(user);

            TempData["Success"] = roles.Contains("Admin")
                ? "Admin role removed."
                : "Admin role assigned.";

            return RedirectToAction("Index");
        }

        // GET /Admin/User/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST /Admin/User/Edit/id
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model, IFormFile? profilePhotoFile)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var (success, message) = await _userRepo.UpdateUserAsync(user, model, profilePhotoFile, _env.WebRootPath);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Detail", new { id });
            }

            ModelState.AddModelError("", message);
            return View(model);
        }

        // POST /Admin/User/ResetPassword/id - admin password change
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters long.";
                return RedirectToAction("Edit", new { id });
            }

            var result = await _userRepo.AdminResetPasswordAsync(user, newPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password changed successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Edit", new { id });
        }
    }
}