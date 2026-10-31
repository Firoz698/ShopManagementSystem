using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.Services;
using ShopManagementSystem.ViewModels;
using System.Security.Cryptography;

namespace ShopManagementSystem.Repository.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly ISmsSender _smsSender;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
            _smsSender = smsSender;
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel vm)
        {
            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                Email = vm.Email,
                UserName = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                Gender = vm.Gender,
                DateOfBirth = vm.DateOfBirth,
                UserType = "Customer",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await EnsureRoleExistsAsync("Customer");
                await _userManager.AddToRoleAsync(user, "Customer");
            }

            return result;
        }

        public async Task SignInAfterRegisterAsync(ApplicationUser user)
            => await _signInManager.SignInAsync(user, isPersistent: false);

        public async Task<SignInResult> LoginAsync(LoginViewModel vm)
            => await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();

        public async Task<bool> IsAdminAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "Admin");
        }

        // Check if user is Admin or Employee by email
        public async Task<bool> IsAdminOrEmployeeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            return await IsUserAdminOrEmployeeAsync(user);
        }

        // Change Password (when logged in)
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        }

        // Generate OTP and send via SMS
        public async Task<(bool Success, string Message)> GenerateOtpAsync(string phoneNumber)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            if (user == null)
                return (false, "No account found with this phone number.");

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var otp = new OtpVerification
            {
                PhoneNumber = phoneNumber,
                Code = otpCode,
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            _db.OtpVerifications.Add(otp);
            await _db.SaveChangesAsync();

            var message = $"Your OTP code is: {otpCode}. Valid for 10 minutes.";
            var sent = await _smsSender.SendSmsAsync(phoneNumber, message);

            return sent
                ? (true, "OTP has been sent to your phone.")
                : (false, "Failed to send OTP. Please try again.");
        }

        // OTP verify
        public async Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode)
        {
            var otp = await _db.OtpVerifications
                .Where(o => o.PhoneNumber == phoneNumber && o.Code == otpCode && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            return otp != null && otp.ExpiresAt >= DateTime.Now;
        }

        // Verify OTP and reset password
        public async Task<IdentityResult> ResetPasswordWithOtpAsync(ResetPasswordViewModel vm)
        {
            var otp = await _db.OtpVerifications
                .Where(o => o.PhoneNumber == vm.PhoneNumber && o.Code == vm.OtpCode && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null || otp.ExpiresAt < DateTime.Now)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid or expired OTP." });

            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == vm.PhoneNumber);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (result.Succeeded)
            {
                otp.IsUsed = true;
                await _db.SaveChangesAsync();
            }

            return result;
        }

        public async Task<ApplicationUser?> GetProfileAsync(string userId)
            => await _userManager.FindByIdAsync(userId);

        // Profile update + photo upload
        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, EditProfileViewModel vm, string webRootPath)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;
            user.Gender = vm.Gender;
            user.DateOfBirth = vm.DateOfBirth;

            if (vm.ProfilePhotoFile != null && vm.ProfilePhotoFile.Length > 0)
            {
                var folder = Path.Combine(webRootPath, "uploads", "profile");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.ProfilePhotoFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ProfilePhotoFile.CopyToAsync(stream);
                }

                user.ProfilePhoto = $"/uploads/profile/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? (true, "Profile updated successfully.") : (false, "Profile update failed.");
        }

        // Find external user by login provider
        public async Task<ApplicationUser?> FindByLoginAsync(string provider, string providerKey)
            => await _userManager.FindByLoginAsync(provider, providerKey);

        // External login (Google/Facebook) sign in
        public async Task<SignInResult> ExternalLoginSignInAsync(string provider, string providerKey)
            => await _signInManager.ExternalLoginSignInAsync(provider, providerKey, isPersistent: true);

        // Create external user and link login
        public async Task<IdentityResult> CreateExternalUserAsync(ApplicationUser user, string provider, string providerKey)
        {
            user.UserType = "Customer";

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded) return createResult;

            await EnsureRoleExistsAsync("Customer");
            await _userManager.AddToRoleAsync(user, "Customer");

            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

            return addLoginResult;
        }

        public async Task<bool> IsUserAdminAsync(ApplicationUser user)
            => await _userManager.IsInRoleAsync(user, "Admin");

        // Check if user is Admin or Employee
        public async Task<bool> IsUserAdminOrEmployeeAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return true;

            return user.UserType == "Employee";
        }
    }
}