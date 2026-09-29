using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementSystem.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        // Bootstrap icon class, e.g.: "bi bi-speedometer2"
        [MaxLength(100)]
        public string? Icon { get; set; }

        // Controller/Action/Area - If empty, this is a group header (not clickable)
        [MaxLength(100)]
        public string? Controller { get; set; }

        [MaxLength(100)]
        public string? Action { get; set; } = "Index";

        [MaxLength(100)]
        public string? Area { get; set; } = "Admin";

        // Parent menu (group header) - null indicates top-level group
        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ParentId")]
        public Menu? Parent { get; set; }

        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<UserMenuPermission> UserPermissions { get; set; } = new List<UserMenuPermission>();

        // If Controller is null/empty, this is a group header, not a clickable link
        [NotMapped]
        public bool IsGroupHeader => string.IsNullOrEmpty(Controller);
    }
}