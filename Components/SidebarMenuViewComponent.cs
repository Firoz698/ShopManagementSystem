using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Components
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly ISidebarMenuService _sidebarService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SidebarMenuViewComponent(ISidebarMenuService sidebarService, UserManager<ApplicationUser> userManager)
        {
            _sidebarService = sidebarService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User) ?? string.Empty;
            var tree = await _sidebarService.GetVisibleMenuTreeAsync(HttpContext.User, userId);
            return View(tree);
        }
    }
}