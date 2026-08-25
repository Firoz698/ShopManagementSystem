using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementSystem.Models
{
    public class UserMenuPermission
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int MenuId { get; set; }

        // ── এখন আলাদা আলাদা CRUD পারমিশন ──
        public bool CanAccess { get; set; } = true;   // View / মেনু দেখতে পারবে কিনা
        public bool CanCreate { get; set; } = false;  // নতুন তৈরি করতে পারবে কিনা
        public bool CanEdit { get; set; } = false;    // এডিট/আপডেট করতে পারবে কিনা
        public bool CanDelete { get; set; } = false;  // ডিলিট করতে পারবে কিনা

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("MenuId")]
        public Menu? Menu { get; set; }
    }
}