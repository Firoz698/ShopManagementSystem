using Microsoft.AspNetCore.Identity;
using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

public interface IAccountRepository
{
    Task<IdentityResult> RegisterAsync(RegisterViewModel vm);
    Task SignInAfterRegisterAsync(ApplicationUser user);
    Task<SignInResult> LoginAsync(LoginViewModel vm);
    Task LogoutAsync();
    Task<bool> IsAdminAsync(string email);
    Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel vm);
    Task<(bool Success, string Message)> GenerateOtpAsync(string phoneNumber);
    Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode);
    Task<IdentityResult> ResetPasswordWithOtpAsync(ResetPasswordViewModel vm);
    Task<ApplicationUser?> GetProfileAsync(string userId);
    Task<(bool Success, string Message)> UpdateProfileAsync(string userId, EditProfileViewModel vm, string webRootPath);

    // ✅ notun — External login
    Task<ApplicationUser?> FindByLoginAsync(string provider, string providerKey);
    Task<SignInResult> ExternalLoginSignInAsync(string provider, string providerKey);
    Task<IdentityResult> CreateExternalUserAsync(ApplicationUser user, string provider, string providerKey);
    Task<bool> IsUserAdminAsync(ApplicationUser user);
    Task<bool> IsAdminOrEmployeeAsync(string email);
    Task<bool> IsUserAdminOrEmployeeAsync(ApplicationUser user);
}