using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using System.Net;
using System.Net.Mail;

namespace ShopManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<NotificationService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendNewOrderNotificationAsync(int orderId, string customerName, decimal totalAmount)
        {
            var setting = await _context.NotificationSettings.FirstOrDefaultAsync();
            if (setting == null || !setting.NotifyOnNewOrder) return;

            var subject = $"New Order #{orderId}";
            var body = $"Customer: {customerName}\nOrder Number: #{orderId}\nTotal Amount: ৳{totalAmount}\n\nPlease check the admin panel for details.";
            var smsText = $"New Order #{orderId} - {customerName} - ৳{totalAmount}";

            await SendAsync(setting, subject, body, smsText);
        }

        public async Task SendNewChatMessageNotificationAsync(string customerName, string messageText)
        {
            var setting = await _context.NotificationSettings.FirstOrDefaultAsync();
            if (setting == null || !setting.NotifyOnNewChatMessage) return;

            var subject = $"New Message - {customerName}";
            var body = $"Customer: {customerName}\nMessage: {messageText}\n\nPlease log in to admin panel to reply.";
            var smsText = $"New chat message - {customerName}: {Truncate(messageText, 100)}";

            await SendAsync(setting, subject, body, smsText);
        }

        private async Task SendAsync(NotificationSetting setting, string subject, string emailBody, string smsText)
        {
            if (setting.EmailEnabled && !string.IsNullOrWhiteSpace(setting.AdminEmail))
            {
                try { await SendEmailAsync(setting, subject, emailBody); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to send email notification."); }
            }

            if (setting.SmsEnabled && !string.IsNullOrWhiteSpace(setting.AdminPhone))
            {
                try { await SendSmsAsync(setting, smsText); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to send SMS notification."); }
            }
        }

        private async Task SendEmailAsync(NotificationSetting setting, string subject, string body)
        {
            using var client = new SmtpClient(setting.SmtpHost, setting.SmtpPort)
            {
                Credentials = new NetworkCredential(setting.SmtpUsername, setting.SmtpPassword),
                EnableSsl = setting.SmtpUseSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(setting.SenderEmail ?? setting.SmtpUsername ?? "", setting.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(setting.AdminEmail);

            await client.SendMailAsync(mail);
        }

        private async Task SendSmsAsync(NotificationSetting setting, string message)
        {
            if (string.IsNullOrWhiteSpace(setting.SmsApiUrl)) return;

            var client = _httpClientFactory.CreateClient();

            // Generic GET-style SMS gateway format
            var url = $"{setting.SmsApiUrl}?api_key={setting.SmsApiKey}&senderid={setting.SmsSenderId}" +
                      $"&number={setting.AdminPhone}&message={Uri.EscapeDataString(message)}";

            await client.GetAsync(url);
        }

        private static string Truncate(string text, int maxLength) =>
            text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }
}