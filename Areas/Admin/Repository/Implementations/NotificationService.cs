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

            var subject = $"নতুন অর্ডার #{orderId}";
            var body = $"গ্রাহক: {customerName}\nঅর্ডার নম্বর: #{orderId}\nমোট মূল্য: ৳{totalAmount}\n\nঅ্যাডমিন প্যানেলে গিয়ে বিস্তারিত দেখুন।";
            var smsText = $"নতুন অর্ডার #{orderId} - {customerName} - ৳{totalAmount}";

            await SendAsync(setting, subject, body, smsText);
        }

        public async Task SendNewChatMessageNotificationAsync(string customerName, string messageText)
        {
            var setting = await _context.NotificationSettings.FirstOrDefaultAsync();
            if (setting == null || !setting.NotifyOnNewChatMessage) return;

            var subject = $"নতুন মেসেজ - {customerName}";
            var body = $"গ্রাহক: {customerName}\nমেসেজ: {messageText}\n\nঅ্যাডমিন প্যানেলে গিয়ে রিপ্লাই দিন।";
            var smsText = $"নতুন চ্যাট মেসেজ - {customerName}: {Truncate(messageText, 100)}";

            await SendAsync(setting, subject, body, smsText);
        }

        private async Task SendAsync(NotificationSetting setting, string subject, string emailBody, string smsText)
        {
            if (setting.EmailEnabled && !string.IsNullOrWhiteSpace(setting.AdminEmail))
            {
                try { await SendEmailAsync(setting, subject, emailBody); }
                catch (Exception ex) { _logger.LogError(ex, "Email notification পাঠাতে ব্যর্থ হয়েছে।"); }
            }

            if (setting.SmsEnabled && !string.IsNullOrWhiteSpace(setting.AdminPhone))
            {
                try { await SendSmsAsync(setting, smsText); }
                catch (Exception ex) { _logger.LogError(ex, "SMS notification পাঠাতে ব্যর্থ হয়েছে।"); }
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

            // এটা একটা generic GET-style SMS gateway format (bulksmsbd/adnsms টাইপ প্রোভাইডারদের সাথে মিলবে)
            // আপনার actual SMS provider এর API doc অনুযায়ী url format ঠিক করে নিতে হবে
            var url = $"{setting.SmsApiUrl}?api_key={setting.SmsApiKey}&senderid={setting.SmsSenderId}" +
                      $"&number={setting.AdminPhone}&message={Uri.EscapeDataString(message)}";

            await client.GetAsync(url);
        }

        private static string Truncate(string text, int maxLength) =>
            text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }
}