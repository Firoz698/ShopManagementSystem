using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminEmployeeRepository
    {
        Task<List<ApplicationUser>> GetEmployeesAsync();
        Task<Dictionary<int, UserMenuPermission>> GetPermissionMapAsync(string userId); // ✅ পরিবর্তিত
        Task SaveMenuPermissionsAsync(string userId, List<MenuPermissionInput> permissions); // ✅ পরিবর্তিত
        Task<List<Menu>> GetAssignableMenusAsync();
    }
}