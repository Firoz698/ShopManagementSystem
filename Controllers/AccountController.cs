using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;
using System.Security.Claims;

namespace ShopManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IWebHostEnvironment _env;

        public AccountController(IAccountRepository accountRepo, IWebHostEnvironment env)
        {
            _accountRepo = accountRepo;
            _env = env;
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _accountRepo.RegisterAsync(vm);

            if (result.Succeeded)
            {
                TempData["Success"] = "Welcome! Your account has been created successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _accountRepo.LoginAsync(vm);

            if (result.Succeeded)
            {
                if (await _accountRepo.IsAdminAsync(vm.Email))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountRepo.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/AccessDenied
        public IActionResult AccessDenied() => View();

        // Change Password
        [Authorize, HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _accountRepo.ChangePasswordAsync(userId, vm);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password has been changed successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // Forgot Password -> Generate OTP (SMS)
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (success, message) = await _accountRepo.GenerateOtpAsync(vm.PhoneNumber);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(vm);
            }

            TempData["Info"] = message;
            return RedirectToAction("VerifyOtp", new { phoneNumber = vm.PhoneNumber });
        }

        // OTP verify
        [HttpGet]
        public IActionResult VerifyOtp(string phoneNumber) => View(new VerifyOtpViewModel { PhoneNumber = phoneNumber });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var isValid = await _accountRepo.VerifyOtpAsync(vm.PhoneNumber, vm.OtpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP code or it has expired.");
                return View(vm);
            }

            return RedirectToAction("ResetPassword", new { phoneNumber = vm.PhoneNumber, otpCode = vm.OtpCode });
        }

        // Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string phoneNumber, string otpCode)
            => View(new ResetPasswordViewModel { PhoneNumber = phoneNumber, OtpCode = otpCode });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _accountRepo.ResetPasswordWithOtpAsync(vm);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password has been reset. You can now login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // Profile
        [Authorize, HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _accountRepo.GetProfileAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        // Profile Edit
        [Authorize, HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _accountRepo.GetProfileAsync(userId);
            if (user == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                CurrentPhotoUrl = user.ProfilePhoto
            };
            return View(vm);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, message) = await _accountRepo.UpdateProfileAsync(userId, vm, _env.WebRootPath);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(vm);
            }

            TempData["Success"] = message;
            return RedirectToAction("Profile");
        }

        // External Login
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["Error"] = "External login failed.";
                return RedirectToAction("Login");
            }

            var info = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme);
            if (info?.Principal == null)
            {
                TempData["Error"] = "External login details not found.";
                return RedirectToAction("Login");
            }

            var provider = info.Properties?.Items[".AuthScheme"] ?? "";
            var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var existingUser = await _accountRepo.FindByLoginAsync(provider, providerKey);

            if (existingUser != null)
            {
                var signInResult = await _accountRepo.ExternalLoginSignInAsync(provider, providerKey);
                if (signInResult.Succeeded)
                {
                    if (await _accountRepo.IsUserAdminAsync(existingUser))
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                    return LocalRedirect(returnUrl ?? "/");
                }
            }

            var newUser = new ApplicationUser
            {
                FullName = name,
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var createResult = await _accountRepo.CreateExternalUserAsync(newUser, provider, providerKey);
            if (createResult.Succeeded)
            {
                await _accountRepo.ExternalLoginSignInAsync(provider, providerKey);
                TempData["Success"] = "Welcome! Your account has been created successfully.";
                return LocalRedirect(returnUrl ?? "/");
            }

            TempData["Error"] = "Failed to create account.";
            return RedirectToAction("Login");
        }
    }
}