using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(RoleManager<IdentityRole> roleManager) => _roleManager = roleManager;

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(roles);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "রোলের নাম লিখুন।";
                return RedirectToAction("Index");
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = "এই রোল আগে থেকেই আছে।";
                return RedirectToAction("Index");
            }

            await _roleManager.CreateAsync(new IdentityRole(roleName));
            TempData["Success"] = "রোল তৈরি হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Admin/Employee — core role ডিলিট হতে দেওয়া যাবে না
            if (role.Name == "Admin" || role.Name == "Employee" || role.Name == "Customer")
            {
                TempData["Error"] = "এই ডিফল্ট রোল ডিলিট করা যাবে না।";
                return RedirectToAction("Index");
            }

            await _roleManager.DeleteAsync(role);
            TempData["Success"] = "রোল ডিলিট হয়েছে।";
            return RedirectToAction("Index");
        }
    }
}