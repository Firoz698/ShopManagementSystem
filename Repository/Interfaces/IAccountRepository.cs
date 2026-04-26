using Microsoft.AspNetCore.Identity;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel vm);
        Task<SignInResult> LoginAsync(LoginViewModel vm);
        Task LogoutAsync();
        Task<bool> IsAdminAsync(string email);
        Task SignInAfterRegisterAsync(ApplicationUser user);
    }
}
