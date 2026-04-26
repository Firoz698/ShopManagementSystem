using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementSystem.Models
{
    //public class ReturnRequest
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    public int OrderId { get; set; }

    //    [Required]
    //    public int ProductId { get; set; }

    //    [Required]
    //    public string UserId { get; set; } = string.Empty;

    //    [Required, MaxLength(500)]
    //    public string Reason { get; set; } = string.Empty;

    //    [MaxLength(50)]
    //    public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected

    //    public string? AdminNote { get; set; }

    //    public DateTime CreatedAt { get; set; } = DateTime.Now;
    //    public DateTime? UpdatedAt { get; set; }

    //    [ForeignKey("OrderId")]
    //    public Order? Order { get; set; }

    //    [ForeignKey("ProductId")]
    //    public Product? Product { get; set; }

    //    [ForeignKey("UserId")]
    //    public ApplicationUser? User { get; set; }
    //}
}
