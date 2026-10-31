using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository
{
    public class AdminNotificationSettingRepository : IAdminNotificationSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminNotificationSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationSetting> GetSettingsAsync()
        {
            var setting = await _context.NotificationSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                setting = new NotificationSetting
                {
                    AdminEmail = "",
                    AdminPhone = "",
                    NotifyOnNewOrder = true,
                    NotifyOnNewChatMessage = true
                };
                _context.NotificationSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return setting;
        }

        public async Task UpdateSettingsAsync(NotificationSetting setting)
        {
            var existing = await _context.NotificationSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                setting.UpdatedAt = DateTime.Now;
                _context.NotificationSettings.Add(setting);
            }
            else
            {
                existing.EmailEnabled = setting.EmailEnabled;
                existing.SmtpHost = setting.SmtpHost;
                existing.SmtpPort = setting.SmtpPort;
                existing.SmtpUsername = setting.SmtpUsername;

 // password field submit 
                if (!string.IsNullOrWhiteSpace(setting.SmtpPassword))
                    existing.SmtpPassword = setting.SmtpPassword;

                existing.SmtpUseSsl = setting.SmtpUseSsl;
                existing.SenderEmail = setting.SenderEmail;
                existing.SenderName = setting.SenderName;

                existing.SmsEnabled = setting.SmsEnabled;
                existing.SmsApiUrl = setting.SmsApiUrl;

                if (!string.IsNullOrWhiteSpace(setting.SmsApiKey))
                    existing.SmsApiKey = setting.SmsApiKey;

                existing.SmsSenderId = setting.SmsSenderId;
                existing.AdminEmail = setting.AdminEmail;
                existing.AdminPhone = setting.AdminPhone;
                existing.NotifyOnNewOrder = setting.NotifyOnNewOrder;
                existing.NotifyOnNewChatMessage = setting.NotifyOnNewChatMessage;
                existing.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
