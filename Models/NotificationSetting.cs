using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.Models
{
    public class NotificationSetting
    {
        public int Id { get; set; }

        // ── Email (SMTP) Settings ──
        public bool EmailEnabled { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; } = true;
        public string? SenderEmail { get; set; }
        public string? SenderName { get; set; } = "Shop Notification";

        // ── SMS Settings ──
        public bool SmsEnabled { get; set; }
        public string? SmsApiUrl { get; set; }
        public string? SmsApiKey { get; set; }
        public string? SmsSenderId { get; set; }

        // ── Notification কোথায় যাবে ──
        [Required]
        public string AdminEmail { get; set; } = string.Empty;
        [Required]
        public string AdminPhone { get; set; } = string.Empty;

        // ── কোন কোন event এ পাঠাবে ──
        public bool NotifyOnNewOrder { get; set; } = true;
        public bool NotifyOnNewChatMessage { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }
}