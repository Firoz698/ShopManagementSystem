using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminEmployeeRepository
    {
        Task<List<ApplicationUser>> GetEmployeesAsync();
        Task<Dictionary<int, UserMenuPermission>> GetPermissionMapAsync(string userId); // ✅
        Task SaveMenuPermissionsAsync(string userId, List<MenuPermissionInput> permissions); // ✅
        Task<List<Menu>> GetAssignableMenusAsync();
    }
}
