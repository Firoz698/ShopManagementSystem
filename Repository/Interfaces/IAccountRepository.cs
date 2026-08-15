using Microsoft.AspNetCore.Identity;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel vm);
        Task SignInAfterRegisterAsync(ApplicationUser user);
        Task<SignInResult> LoginAsync(LoginViewModel vm);
        Task LogoutAsync();
        Task<bool> IsAdminAsync(string email);

        // ✅ notun
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel vm);
        Task<(bool Success, string Message)> GenerateOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode);
        Task<IdentityResult> ResetPasswordWithOtpAsync(ResetPasswordViewModel vm);
        Task<ApplicationUser?> GetProfileAsync(string userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, EditProfileViewModel vm, string webRootPath);
    }
}