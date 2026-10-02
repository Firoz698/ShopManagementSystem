using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Filters
{
    public class MenuPermissionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        // These controllers are always accessible by Employees (no explicit permission needed)
        private static readonly string[] AlwaysAllowed = { "Dashboard", "Account" };

        // Actions requiring Create permission
        private static readonly string[] CreateKeywords = { "Create", "Add" };
        // Actions requiring Edit permission
        private static readonly string[] EditKeywords = { "Edit", "Update", "Toggle", "Move", "Permissions", "ResetPassword" };
        // Actions requiring Delete permission
        private static readonly string[] DeleteKeywords = { "Delete", "Remove" };

        public MenuPermissionFilter(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var area = context.RouteData.Values["area"]?.ToString();
            if (area != "Admin") { await next(); return; }

            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated) { await next(); return; }

            if (user.IsInRole("Admin")) { await next(); return; } // Admin has full access

            var userId = _userManager.GetUserId(user);
            if (!string.IsNullOrEmpty(userId))
            {
                var appUser = await _db.Users.FindAsync(userId);
                if (appUser != null && (appUser.UserType == "Admin" || appUser.Email == "admin@shopmanagement.com"))
                {
                    await next();
                    return;
                }
            }

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            if (AlwaysAllowed.Contains(controller, StringComparer.OrdinalIgnoreCase)) { await next(); return; }

            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            // Find Menu corresponding to this controller
            var menu = await _db.Menus
                .Where(m => m.Controller == controller && m.Area == "Admin")
                .Select(m => new { m.Id })
                .FirstOrDefaultAsync();

            if (menu == null) { await Deny(context); return; } // Block if menu not found

            var permission = await _db.UserMenuPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MenuId == menu.Id);

            // Block if user cannot access this menu
            if (permission == null || !permission.CanAccess) { await Deny(context); return; }

            // Check specific CRUD permission by action name
            bool needsCreate = CreateKeywords.Any(k => action.Contains(k, StringComparison.OrdinalIgnoreCase));
            bool needsEdit = EditKeywords.Any(k => action.Contains(k, StringComparison.OrdinalIgnoreCase));
            bool needsDelete = DeleteKeywords.Any(k => action.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (needsCreate && !permission.CanCreate) { await Deny(context); return; }
            if (needsEdit && !permission.CanEdit) { await Deny(context); return; }
            if (needsDelete && !permission.CanDelete) { await Deny(context); return; }

            await next();
        }

        private Task Deny(ActionExecutingContext context)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Dashboard", new { area = "Admin" });
            return Task.CompletedTask;
        }
    }
}