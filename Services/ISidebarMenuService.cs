using System.Security.Claims;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Services
{
    public interface ISidebarMenuService
    {
        Task<List<Menu>> GetVisibleMenuTreeAsync(ClaimsPrincipal user, string userId);
    }
}