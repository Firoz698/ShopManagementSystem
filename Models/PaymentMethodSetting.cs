using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.Models
{
    public class PaymentMethodSetting
    {
        public int Id { get; set; }

        // যেমন: bKash, Nagad, Rocket, Bank Transfer
        [Required, MaxLength(100)]
        public string MethodName { get; set; } = string.Empty;

        // Personal / Merchant / Bank — কাস্টমারকে বোঝানোর জন্য
        [MaxLength(50)]
        public string? AccountType { get; set; }

        // Wallet Number অথবা Bank Account Number
        [Required, MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? AccountHolderName { get; set; }

        // শুধু Bank Transfer হলে কাজে লাগবে
        [MaxLength(150)]
        public string? BankName { get; set; }
        [MaxLength(150)]
        public string? BranchName { get; set; }
        [MaxLength(100)]
        public string? RoutingOrSwift { get; set; }

        // কাস্টমারকে কীভাবে পেমেন্ট করতে হবে তার নির্দেশনা
        [MaxLength(1000)]
        public string? Instructions { get; set; }

        // লিস্টে/checkout এ আইকন দেখানোর জন্য (optional)
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}