using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // সব user — নতুন থেকে পুরনো
        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // প্রতিটি user এর প্রথম role Dictionary তে রাখে
        public async Task<Dictionary<string, string>> GetUserRolesAsync(List<ApplicationUser> users)
        {
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "User";
            }
            return userRoles;
        }

        // Id দিয়ে user খোঁজে
        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        // User এর সব order — details সহ, নতুন থেকে পুরনো
        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // User এর role list
        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        // User delete এর আগে Cart ও Wishlist মুছে ফেলে
        public async Task RemoveUserRelatedDataAsync(string userId)
        {
            var carts = _db.Carts.Where(c => c.UserId == userId);
            var wishlists = _db.Wishlists.Where(w => w.UserId == userId);
            _db.Carts.RemoveRange(carts);
            _db.Wishlists.RemoveRange(wishlists);
            await _db.SaveChangesAsync();
        }

        // User delete করে — true হলে সফল
        public async Task<bool> DeleteUserAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        // Admin হলে User করে, User হলে Admin করে
        public async Task ToggleRoleAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // User info update করে — true হলে সফল
        public async Task<bool> UpdateUserAsync(ApplicationUser user, ApplicationUser model)
        {
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
