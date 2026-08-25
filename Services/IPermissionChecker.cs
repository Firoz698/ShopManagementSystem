using System.Security.Claims;

namespace ShopManagementSystem.Services
{
    public interface IPermissionChecker
    {
        Task<(bool CanView, bool CanCreate, bool CanEdit, bool CanDelete)> GetPermissionsAsync(
            ClaimsPrincipal user, string controller);
    }
}