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
        Task<bool> UpdateUserAsync(ApplicationUser user, ApplicationUser model);
    }
}
