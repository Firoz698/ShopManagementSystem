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
                TempData["Error"] = "Please enter a role name.";
                return RedirectToAction("Index");
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = "This role already exists.";
                return RedirectToAction("Index");
            }

            await _roleManager.CreateAsync(new IdentityRole(roleName));
            TempData["Success"] = "Role created successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Core roles cannot be deleted
            if (role.Name == "Admin" || role.Name == "Employee" || role.Name == "Customer")
            {
                TempData["Error"] = "This default role cannot be deleted.";
                return RedirectToAction("Index");
            }

            await _roleManager.DeleteAsync(role);
            TempData["Success"] = "Role deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}