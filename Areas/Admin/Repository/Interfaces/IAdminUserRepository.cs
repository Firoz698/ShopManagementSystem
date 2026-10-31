using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminUserRepository
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<Dictionary<string, string>> GetUserRolesAsync(List<ApplicationUser> users);
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task RemoveUserRelatedDataAsync(string userId);
        Task<bool> DeleteUserAsync(ApplicationUser user);
        Task ToggleRoleAsync(ApplicationUser user);

        // ✅ Update — email, photo shoho sob property
        Task<(bool Success, string Message)> UpdateUserAsync(
            ApplicationUser user, ApplicationUser model, IFormFile? photoFile, string webRootPath);

        // ✅ Admin diye password reset (current password lagbe na)
        Task<Microsoft.AspNetCore.Identity.IdentityResult> AdminResetPasswordAsync(ApplicationUser user, string newPassword);
    }
}