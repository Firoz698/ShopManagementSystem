using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepo;

        public AccountController(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
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
                // Register এর পর auto login এর জন্য email দিয়ে user খুঁজে sign-in
                var fakeUser = new ShopManagementSystem.Models.ApplicationUser
                {
                    Email = vm.Email,
                    UserName = vm.Email,
                    FullName = vm.FullName
                };
                await _accountRepo.SignInAfterRegisterAsync(fakeUser);

                TempData["Success"] = "স্বাগতম! অ্যাকাউন্ট তৈরি হয়েছে।";
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

            ModelState.AddModelError(string.Empty, "ইমেইল বা পাসওয়ার্ড সঠিক নয়।");
            return View(vm);
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountRepo.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }

}
