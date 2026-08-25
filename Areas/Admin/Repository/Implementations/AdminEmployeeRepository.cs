using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Repository
{
    public class AdminEmployeeRepository : IAdminEmployeeRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminEmployeeRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<ApplicationUser>> GetEmployeesAsync()
        {
            return await _db.Users
                .Where(u => u.UserType == "Employee")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // ✅ MenuId → UserMenuPermission ম্যাপ, যাতে View তে সহজে CanView/CanCreate/CanEdit/CanDelete চেক করা যায়
        public async Task<Dictionary<int, UserMenuPermission>> GetPermissionMapAsync(string userId)
        {
            var list = await _db.UserMenuPermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();
            return list.ToDictionary(p => p.MenuId, p => p);
        }

        // ✅ পুরো permission সেট রিপ্লেস করে (delete + re-insert, simple ও নিরাপদ)
        public async Task SaveMenuPermissionsAsync(string userId, List<MenuPermissionInput> permissions)
        {
            var existing = await _db.UserMenuPermissions.Where(p => p.UserId == userId).ToListAsync();
            _db.UserMenuPermissions.RemoveRange(existing);

            foreach (var p in permissions.Where(p => p.CanView || p.CanCreate || p.CanEdit || p.CanDelete))
            {
                _db.UserMenuPermissions.Add(new UserMenuPermission
                {
                    UserId = userId,
                    MenuId = p.MenuId,
                    CanAccess = p.CanView,
                    CanCreate = p.CanCreate,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<Menu>> GetAssignableMenusAsync()
        {
            return await _db.Menus
                .Where(m => m.IsActive && m.Controller != null)
                .Include(m => m.Parent)
                .OrderBy(m => m.ParentId ?? 0).ThenBy(m => m.SortOrder)
                .ToListAsync();
        }
    }
}