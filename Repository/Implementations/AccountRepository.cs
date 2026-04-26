using Microsoft.AspNetCore.Identity;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // নতুন User তৈরি করে "User" role assign করে
        public async Task<IdentityResult> RegisterAsync(RegisterViewModel vm)
        {
            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                Email = vm.Email,
                UserName = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return result;
        }

        // Register সফল হলে auto sign-in করার জন্য
        public async Task SignInAfterRegisterAsync(ApplicationUser user)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        // Email দিয়ে login করে result return করে
        public async Task<SignInResult> LoginAsync(LoginViewModel vm)
        {
            return await _signInManager.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: false
            );
        }

        // Logout করে
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // User Admin কিনা check করে
        public async Task<bool> IsAdminAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "Admin");
        }
    }
}
