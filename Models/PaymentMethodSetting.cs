using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.Models
{
    public class PaymentMethodSetting
    {
        public int Id { get; set; }

        // E.g.: bKash, Nagad, Rocket, Bank Transfer
        [Required, MaxLength(100)]
        public string MethodName { get; set; } = string.Empty;

        // Personal / Merchant / Bank
        [MaxLength(50)]
        public string? AccountType { get; set; }

        // Wallet Number or Bank Account Number
        [Required, MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? AccountHolderName { get; set; }

        // Bank Transfer specific fields
        [MaxLength(150)]
        public string? BankName { get; set; }
        [MaxLength(150)]
        public string? BranchName { get; set; }
        [MaxLength(100)]
        public string? RoutingOrSwift { get; set; }

        // Payment instructions for customers
        [MaxLength(1000)]
        public string? Instructions { get; set; }

        // Icon/logo for checkout display (optional)
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}