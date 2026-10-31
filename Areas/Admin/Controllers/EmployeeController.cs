using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class EmployeeController : Controller
    {
        private readonly IAdminEmployeeRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(
            IAdminEmployeeRepository repo,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _repo = repo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _repo.GetEmployeesAsync();
            return View(employees);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(new CreateEmployeeViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View(vm);
            }

            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                UserName = vm.Email,
                Email = vm.Email,
                PhoneNumber = vm.Phone,
                UserType = "Employee",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View(vm);
            }

            if (!string.IsNullOrEmpty(vm.RoleName))
                await _userManager.AddToRoleAsync(user, vm.RoleName);

            TempData["Success"] = "Employee created successfully. Please configure menu permissions.";
            return RedirectToAction("Permissions", new { id = user.Id });
        }

        public async Task<IActionResult> Permissions(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allMenus = await _repo.GetAssignableMenusAsync();
            var permMap = await _repo.GetPermissionMapAsync(id);

            ViewBag.Employee = user;
            ViewBag.PermissionMap = permMap; // MenuId -> UserMenuPermission
            return View(allMenus);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(string userId, List<MenuPermissionInput> permissions)
        {
            await _repo.SaveMenuPermissionsAsync(userId, permissions ?? new List<MenuPermissionInput>());
            TempData["Success"] = "Permissions saved successfully.";
            return RedirectToAction("Index");
        }
    }
}