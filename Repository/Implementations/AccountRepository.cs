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
        private readonly ApplicationDbContext _db;
        private readonly ISmsSender _smsSender;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _smsSender = smsSender;
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
                UserType = "Customer", // ✅ স্পষ্টভাবে সেট করা হলো (default থাকলেও নিশ্চিত করা ভালো)
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "Customer"); // ✅ "User" থেকে "Customer" করা হলো

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

        // ✅ নতুন — Email দিয়ে Admin অথবা Employee কিনা চেক করে
        public async Task<bool> IsAdminOrEmployeeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            return await IsUserAdminOrEmployeeAsync(user);
        }

        // ✅ Change Password (login thakle)
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "ইউজার পাওয়া যায়নি।" });

            return await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        }

        // ✅ OTP generate kore SMS pathay
        public async Task<(bool Success, string Message)> GenerateOtpAsync(string phoneNumber)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            if (user == null)
                return (false, "এই ফোন নম্বরে কোনো অ্যাকাউন্ট পাওয়া যায়নি।");

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var otp = new OtpVerification
            {
                PhoneNumber = phoneNumber,
                Code = otpCode,
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            _db.OtpVerifications.Add(otp);
            await _db.SaveChangesAsync();

            var message = $"আপনার OTP কোড: {otpCode}। এটি ১০ মিনিটের জন্য বৈধ।";
            var sent = await _smsSender.SendSmsAsync(phoneNumber, message);

            return sent
                ? (true, "OTP আপনার ফোনে পাঠানো হয়েছে।")
                : (false, "OTP পাঠাতে সমস্যা হয়েছে, আবার চেষ্টা করুন।");
        }

        // ✅ OTP verify
        public async Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode)
        {
            var otp = await _db.OtpVerifications
                .Where(o => o.PhoneNumber == phoneNumber && o.Code == otpCode && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            return otp != null && otp.ExpiresAt >= DateTime.Now;
        }

        // ✅ OTP verify kore notun password set
        public async Task<IdentityResult> ResetPasswordWithOtpAsync(ResetPasswordViewModel vm)
        {
            var otp = await _db.OtpVerifications
                .Where(o => o.PhoneNumber == vm.PhoneNumber && o.Code == vm.OtpCode && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null || otp.ExpiresAt < DateTime.Now)
                return IdentityResult.Failed(new IdentityError { Description = "OTP সঠিক নয় অথবা মেয়াদ শেষ।" });

            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == vm.PhoneNumber);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "ইউজার পাওয়া যায়নি।" });

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

        // ✅ Profile update + photo upload
        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, EditProfileViewModel vm, string webRootPath)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "ইউজার পাওয়া যায়নি।");

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
            return result.Succeeded ? (true, "প্রোফাইল আপডেট হয়েছে।") : (false, "আপডেট ব্যর্থ হয়েছে।");
        }

        // ✅ Provider + providerKey diye existing external-login user khoje
        public async Task<ApplicationUser?> FindByLoginAsync(string provider, string providerKey)
            => await _userManager.FindByLoginAsync(provider, providerKey);

        // ✅ External login (Google/Facebook) diye sign in
        public async Task<SignInResult> ExternalLoginSignInAsync(string provider, string providerKey)
            => await _signInManager.ExternalLoginSignInAsync(provider, providerKey, isPersistent: true);

        // ✅ Notun external user create kore, provider-er sathe login link kore
        public async Task<IdentityResult> CreateExternalUserAsync(ApplicationUser user, string provider, string providerKey)
        {
            user.UserType = "Customer"; // ✅ স্পষ্টভাবে সেট করা হলো

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded) return createResult;

            await _userManager.AddToRoleAsync(user, "Customer"); // ✅ "User" থেকে "Customer" করা হলো

            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

            return addLoginResult;
        }

        public async Task<bool> IsUserAdminAsync(ApplicationUser user)
            => await _userManager.IsInRoleAsync(user, "Admin");

        // ✅ নতুন — User object দিয়ে Admin অথবা Employee কিনা চেক করে
        public async Task<bool> IsUserAdminOrEmployeeAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return true;

            return user.UserType == "Employee";
        }
    }
}