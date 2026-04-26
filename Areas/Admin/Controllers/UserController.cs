using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAdminUserRepository _userRepo;

        public UserController(IAdminUserRepository userRepo)
        {
            _userRepo = userRepo;
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
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var success = await _userRepo.UpdateUserAsync(user, model);
            if (success)
                return RedirectToAction("Detail", new { id });

            ModelState.AddModelError("", "আপডেট করা সম্ভব হয়নি।");
            return View(model);
        }
    }
}