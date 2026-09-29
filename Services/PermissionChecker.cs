using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionChecker(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<(bool CanView, bool CanCreate, bool CanEdit, bool CanDelete)> GetPermissionsAsync(
            ClaimsPrincipal user, string controller)
        {
 // Admin 
            if (user.IsInRole("Admin"))
                return (true, true, true, true);

            var userId = _userManager.GetUserId(user);
            if (userId == null) return (false, false, false, false);

            var menu = await _db.Menus
                .Where(m => m.Controller == controller && m.Area == "Admin")
                .Select(m => new { m.Id })
                .FirstOrDefaultAsync();

            if (menu == null) return (false, false, false, false);

            var perm = await _db.UserMenuPermissions
                .Where(p => p.UserId == userId && p.MenuId == menu.Id)
                .Select(p => new { p.CanAccess, p.CanCreate, p.CanEdit, p.CanDelete })
                .FirstOrDefaultAsync();

            if (perm == null) return (false, false, false, false);

            return (perm.CanAccess, perm.CanCreate, perm.CanEdit, perm.CanDelete);
        }
    }
}
