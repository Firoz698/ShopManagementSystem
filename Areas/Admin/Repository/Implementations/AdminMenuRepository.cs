using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository
{
    public class AdminMenuRepository : IAdminMenuRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminMenuRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<Menu>> GetAllAsync()
        {
            return await _db.Menus
                .Include(m => m.Parent)
                .OrderBy(m => m.ParentId ?? 0).ThenBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<Menu?> GetByIdAsync(int id) => await _db.Menus.FindAsync(id);

 // (Controller ) — dropdown parent 
        public async Task<List<Menu>> GetHeaderOptionsAsync()
        {
            return await _db.Menus
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task CreateAsync(Menu menu)
        {
            _db.Menus.Add(menu);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Menu menu)
        {
            var existing = await _db.Menus.FindAsync(menu.Id);
            if (existing == null) return;

            existing.Title = menu.Title;
            existing.Icon = menu.Icon;
            existing.Controller = menu.Controller;
            existing.Action = string.IsNullOrWhiteSpace(menu.Action) ? "Index" : menu.Action;
            existing.Area = menu.Area;
            existing.ParentId = menu.ParentId;
            existing.SortOrder = menu.SortOrder;
            existing.IsActive = menu.IsActive;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var menu = await _db.Menus.FindAsync(id);
            if (menu == null) return;

 // (safety)
            var hasChildren = await _db.Menus.AnyAsync(m => m.ParentId == id);
            if (hasChildren) return;

            _db.Menus.Remove(menu);
            await _db.SaveChangesAsync();
        }
    }
}
