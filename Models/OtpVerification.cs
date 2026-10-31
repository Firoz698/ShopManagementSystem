using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.Models
{
    public class OtpVerification
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}