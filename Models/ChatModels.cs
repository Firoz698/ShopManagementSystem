using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementSystem.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string ReceiverId { get; set; } = string.Empty;  // Admin হলে "admin"

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public bool IsFromAdmin { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.Now;

        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }
    }

    public class ChatSession
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public int UnreadCount { get; set; } = 0;
        public DateTime LastMessageAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

}
