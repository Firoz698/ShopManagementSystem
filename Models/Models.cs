using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

namespace ShopManagementSystem.Models
{
    // ── Identity User ───────────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<Wishlist> Wishlist { get; set; } = new List<Wishlist>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    // ── Category ────────────────────────────────────────────────────────────────
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // ── Product ─────────────────────────────────────────────────────────────────
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductVat { get; set; } = 0;

        public bool IsDiscountFixed { get; set; } = true;
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        public int Stock { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    // ── Product Size ─────────────────────────────────────────────────────────────
    public class ProductSize
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public string SizeName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SizeCode { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal SalesPrice { get; set; }

        public int Stock { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Product Image ────────────────────────────────────────────────────────────
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Slider ──────────────────────────────────────────────────────────────────
    public class Slider
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? SubTitle { get; set; }

        [MaxLength(100)]
        public string? ButtonText { get; set; }

        [MaxLength(300)]
        public string? ButtonUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ── Cart ────────────────────────────────────────────────────────────────────
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        // ✅ nullable — Size না থাকলে null হবে, FK error হবে না
        public int? ProductSizeId { get; set; }

        [Required, Range(1, 999)]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("ProductSizeId")]
        public ProductSize? ProductSize { get; set; }
    }

    // ── Wishlist ────────────────────────────────────────────────────────────────
    public class Wishlist
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Order ───────────────────────────────────────────────────────────────────
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        // ✅ নতুন — ঢাকার ভিতর / ঢাকার বাইরে
        [MaxLength(50)]
        public string DeliveryZone { get; set; } = "ঢাকার ভিতর";

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    // ── Order Detail ────────────────────────────────────────────────────────────
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Required, Range(1, 999)]
        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Review ──────────────────────────────────────────────────────────────────
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Return Request ───────────────────────────────────────────────────────────
    public class ReturnRequest
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }

    // ── Payment Transaction ──────────────────────────────────────────────────────
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ValidationId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "BDT";
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }

    // ── Combo Offer ──────────────────────────────────────────────────────────────
    public class ComboOffer
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? BannerImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ComboPrice { get; set; }       // কম্বো মূল্য

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }   // মূল মূল্য (auto-calculated)

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ComboItem> Items { get; set; } = new List<ComboItem>();
    }

    // ── Combo Item (কম্বোর ভেতরে যে প্রোডাক্টগুলো আছে) ─────────────────────────
    public class ComboItem
    {
        public int Id { get; set; }
        public int ComboId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;

        [ForeignKey("ComboId")]
        public ComboOffer? Combo { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    // ── Homepage Section (ডায়নামিক সেকশন) ──────────────────────────────────────
    public class HomepageSection
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SectionType { get; set; } = "category"; // category | combo | product | banner

        public string? SubTitle { get; set; }
        public string? BgColor { get; set; } = "#ffffff";
        public string? TextColor { get; set; } = "#000000";
        public string? BannerImageUrl { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Category section এর জন্য
        public ICollection<HomepageSectionCategory> Categories { get; set; } = new List<HomepageSectionCategory>();

        // Product section এর জন্য
        public ICollection<HomepageSectionProduct> Products { get; set; } = new List<HomepageSectionProduct>();
    }

    public class HomepageSectionCategory
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("SectionId")]
        public HomepageSection? Section { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }

    public class HomepageSectionProduct
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("SectionId")]
        public HomepageSection? Section { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

}
