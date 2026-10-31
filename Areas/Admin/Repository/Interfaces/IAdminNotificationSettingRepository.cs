using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminNotificationSettingRepository
    {
        Task<NotificationSetting> GetSettingsAsync();
        Task UpdateSettingsAsync(NotificationSetting setting);
    }
}