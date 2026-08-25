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

        // এই controller গুলো Employee সবসময় ব্যবহার করতে পারবে (permission ছাড়াই)
        private static readonly string[] AlwaysAllowed = { "Dashboard", "Account" };

        // action নামে এই শব্দগুলো থাকলে Create পারমিশন লাগবে
        private static readonly string[] CreateKeywords = { "Create", "Add" };
        // action নামে এই শব্দগুলো থাকলে Edit পারমিশন লাগবে
        private static readonly string[] EditKeywords = { "Edit", "Update", "Toggle" };
        // action নামে এই শব্দগুলো থাকলে Delete পারমিশন লাগবে
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

            if (user.IsInRole("Admin")) { await next(); return; } // Admin সব পাবে, সব CRUD পারবে

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            if (AlwaysAllowed.Contains(controller)) { await next(); return; }

            var action = context.RouteData.Values["action"]?.ToString() ?? "";
            var userId = _userManager.GetUserId(user);

            // এই controller এর সাথে যুক্ত Menu খুঁজে বের করো
            var menu = await _db.Menus
                .Where(m => m.Controller == controller && m.Area == "Admin")
                .Select(m => new { m.Id })
                .FirstOrDefaultAsync();

            if (menu == null) { await Deny(context); return; } // Menu টেবিলে entry না থাকলে ব্লক

            var permission = await _db.UserMenuPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MenuId == menu.Id);

            // মূল "দেখা" পারমিশনই না থাকলে সরাসরি ব্লক
            if (permission == null || !permission.CanAccess) { await Deny(context); return; }

            // action নাম অনুযায়ী প্রয়োজনীয় CRUD পারমিশন চেক করো
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