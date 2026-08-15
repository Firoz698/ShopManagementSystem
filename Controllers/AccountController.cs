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

        // ✅ GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // ✅ POST /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _accountRepo.RegisterAsync(vm);

            if (result.Succeeded)
            {
                var user = new ApplicationUser
                {
                    Email = vm.Email,
                    UserName = vm.Email,
                    FullName = vm.FullName
                };
                await _accountRepo.SignInAfterRegisterAsync(user);

                TempData["Success"] = "স্বাগতম! অ্যাকাউন্ট তৈরি হয়েছে।";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ✅ GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ✅ POST /Account/Login
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

            ModelState.AddModelError(string.Empty, "ইমেইল বা পাসওয়ার্ড সঠিক নয়।");
            return View(vm);
        }

        // ✅ POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountRepo.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ✅ GET /Account/AccessDenied
        public IActionResult AccessDenied() => View();

        // ✅ Change Password
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
                TempData["Success"] = "পাসওয়ার্ড পরিবর্তন হয়েছে।";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ✅ Forgot Password → OTP পাঠায় (SMS)
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

        // ✅ OTP verify
        [HttpGet]
        public IActionResult VerifyOtp(string phoneNumber) => View(new VerifyOtpViewModel { PhoneNumber = phoneNumber });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var isValid = await _accountRepo.VerifyOtpAsync(vm.PhoneNumber, vm.OtpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "OTP সঠিক নয় অথবা মেয়াদ শেষ হয়ে গেছে।");
                return View(vm);
            }

            return RedirectToAction("ResetPassword", new { phoneNumber = vm.PhoneNumber, otpCode = vm.OtpCode });
        }

        // ✅ Reset Password
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
                TempData["Success"] = "পাসওয়ার্ড রিসেট হয়েছে। এখন লগইন করুন।";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ✅ Profile দেখা
        [Authorize, HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _accountRepo.GetProfileAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        // ✅ Profile Edit
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
    }
}