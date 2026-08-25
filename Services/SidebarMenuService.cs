using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Services
{
    public class SidebarMenuService : ISidebarMenuService
    {
        private readonly ApplicationDbContext _db;

        public SidebarMenuService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Menu>> GetVisibleMenuTreeAsync(ClaimsPrincipal principal, string userId)
        {
            var allMenus = await _db.Menus
                .Where(m => m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();

            bool isAdmin = principal.IsInRole("Admin");

            HashSet<int> allowedIds;
            if (isAdmin)
            {
                // Admin সব active menu দেখবে
                allowedIds = allMenus.Select(m => m.Id).ToHashSet();
            }
            else
            {
                var permitted = await _db.UserMenuPermissions
                    .Where(p => p.UserId == userId && p.CanAccess)
                    .Select(p => p.MenuId)
                    .ToListAsync();
                allowedIds = permitted.ToHashSet();
            }

            // Group header দেখাবে শুধু যদি তার কমপক্ষে ১টা child allowed থাকে
            var visible = new HashSet<int>(allowedIds);
            foreach (var header in allMenus.Where(m => m.IsGroupHeader))
            {
                bool hasVisibleChild = allMenus.Any(c => c.ParentId == header.Id && allowedIds.Contains(c.Id));
                if (hasVisibleChild) visible.Add(header.Id);
            }

            var visibleMenus = allMenus.Where(m => visible.Contains(m.Id)).ToList();

            foreach (var m in visibleMenus)
                m.Children = visibleMenus.Where(c => c.ParentId == m.Id).OrderBy(c => c.SortOrder).ToList();

            return visibleMenus.Where(m => m.ParentId == null).OrderBy(m => m.SortOrder).ToList();
        }
    }
}