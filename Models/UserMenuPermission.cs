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

        // CRUD permissions
        public bool CanAccess { get; set; } = true;   // View / can access menu
        public bool CanCreate { get; set; } = false;  // Can create new items
        public bool CanEdit { get; set; } = false;    // Can edit/update items
        public bool CanDelete { get; set; } = false;  // Can delete items

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("MenuId")]
        public Menu? Menu { get; set; }
    }
}