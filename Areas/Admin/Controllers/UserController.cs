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
                TempData["Success"] = "ইউজার মুছে ফেলা হয়েছে।";
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
                ? "Admin রোল সরানো হয়েছে।"
                : "Admin রোল দেওয়া হয়েছে।";

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

        // ✅ POST /Admin/User/ResetPassword/id — admin diye password change
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "পাসওয়ার্ড কমপক্ষে ৬ অক্ষরের হতে হবে।";
                return RedirectToAction("Edit", new { id });
            }

            var result = await _userRepo.AdminResetPasswordAsync(user, newPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "পাসওয়ার্ড পরিবর্তন হয়েছে।"
                : string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Edit", new { id });
        }
    }
}