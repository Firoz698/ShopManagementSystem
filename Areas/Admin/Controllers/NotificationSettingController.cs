using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class NotificationSettingController : Controller
    {
        private readonly IAdminNotificationSettingRepository _settingRepo;

        public NotificationSettingController(IAdminNotificationSettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        // GET /Admin/NotificationSetting
        public async Task<IActionResult> Index()
        {
            var setting = await _settingRepo.GetSettingsAsync();
            return View(setting);
        }

        // POST /Admin/NotificationSetting
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(NotificationSetting model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _settingRepo.UpdateSettingsAsync(model);
            TempData["Success"] = "Notification settings updated successfully.";
            return RedirectToAction("Index");
        }
    }
}