using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementSystem.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        // Bootstrap icon class, যেমন: "bi bi-speedometer2"
        [MaxLength(100)]
        public string? Icon { get; set; }

        // Controller/Action/Area — খালি থাকলে এটা শুধু গ্রুপ হেডার (ক্লিক করা যাবে না)
        [MaxLength(100)]
        public string? Controller { get; set; }

        [MaxLength(100)]
        public string? Action { get; set; } = "Index";

        [MaxLength(100)]
        public string? Area { get; set; } = "Admin";

        // Parent menu (গ্রুপ হেডার) — null হলে এটা top-level গ্রুপ
        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ParentId")]
        public Menu? Parent { get; set; }

        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<UserMenuPermission> UserPermissions { get; set; } = new List<UserMenuPermission>();

        // Controller না থাকলে এটা শুধু গ্রুপ লেবেল (যেমন "সেটিংস"), ক্লিকযোগ্য লিংক না
        [NotMapped]
        public bool IsGroupHeader => string.IsNullOrEmpty(Controller);
    }
}