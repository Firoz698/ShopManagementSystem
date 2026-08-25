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

        // ✅ শুধু Customer — Employee রা এখানে দেখাবে না (তাদের জন্য আলাদা EmployeeController আছে)
        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _db.Users
                .Where(u => u.UserType == "Customer")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetUserRolesAsync(List<ApplicationUser> users)
        {
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Customer";
            }
            return userRoles;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task RemoveUserRelatedDataAsync(string userId)
        {
            var carts = _db.Carts.Where(c => c.UserId == userId);
            var wishlists = _db.Wishlists.Where(w => w.UserId == userId);
            _db.Carts.RemoveRange(carts);
            _db.Wishlists.RemoveRange(wishlists);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        // ✅ "User" এর বদলে "Customer" role ব্যবহার করা হলো (নতুন role scheme অনুযায়ী)
        public async Task ToggleRoleAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!await _userManager.IsInRoleAsync(user, "Customer"))
                    await _userManager.AddToRoleAsync(user, "Customer");
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "Customer"))
                    await _userManager.RemoveFromRoleAsync(user, "Customer");
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // ⚠️ SecurityStamp ar UserName-er data corrupt/empty thakle direct EF diye thik kore
        // (UserManager bypass kore — karon UserManager-er kono method-e validation trigger hoy,
        //  ar empty UserName thakle sheita age-i fail kore)
        private async Task RepairIdentityFieldsAsync(ApplicationUser user, string fallbackEmail)
        {
            var needsSave = false;

            if (string.IsNullOrEmpty(user.SecurityStamp))
            {
                user.SecurityStamp = Guid.NewGuid().ToString();
                needsSave = true;
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                var repairedName = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : fallbackEmail;
                if (!string.IsNullOrWhiteSpace(repairedName))
                {
                    user.UserName = repairedName;
                    user.NormalizedUserName = repairedName.ToUpperInvariant();
                    needsSave = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(user.Email) && string.IsNullOrWhiteSpace(user.NormalizedEmail))
            {
                user.NormalizedEmail = user.Email.ToUpperInvariant();
                needsSave = true;
            }

            if (needsSave)
            {
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }
        }

        // ✅ Full user update — Email, Photo, sob property shoho
        public async Task<(bool Success, string Message)> UpdateUserAsync(
            ApplicationUser user, ApplicationUser model, IFormFile? photoFile, string webRootPath)
        {
            // ⚠️ Age purono corrupt data (empty UserName/SecurityStamp) thik kore nao
            await RepairIdentityFieldsAsync(user, model.Email);

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;

            // Email change hole UserManager-er proper method diye update koro
            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!emailResult.Succeeded)
                    return (false, string.Join(", ", emailResult.Errors.Select(e => e.Description)));

                var usernameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!usernameResult.Succeeded)
                    return (false, string.Join(", ", usernameResult.Errors.Select(e => e.Description)));
            }

            // Photo upload
            if (photoFile != null && photoFile.Length > 0)
            {
                var folder = Path.Combine(webRootPath, "uploads", "profile");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                user.ProfilePhoto = $"/uploads/profile/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded
                ? (true, "আপডেট সফল হয়েছে।")
                : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // ✅ Admin diye password reset — current password chara-i
        public async Task<IdentityResult> AdminResetPasswordAsync(ApplicationUser user, string newPassword)
        {
            // ⚠️ Age purono corrupt data thik kore nao
            await RepairIdentityFieldsAsync(user, user.Email ?? "");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}