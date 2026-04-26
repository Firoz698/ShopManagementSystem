using System.ComponentModel.DataAnnotations;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.ViewModels
{
    // ── Auth ─────────────────────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    // ── Home / Product ───────────────────────────────────────────────────────────
    public class HomeViewModel
    {
        public List<Slider> Sliders { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<ComboOffer> ComboOffers { get; set; } = new();
        public List<HomepageSection> Sections { get; set; } = new();
    }

    public class ProductListViewModel
    {
        public List<Product>  Products       { get; set; } = new();
        public List<Category> Categories     { get; set; } = new();
        public int?           CategoryId     { get; set; }
        public string?        SearchTerm     { get; set; }
        public decimal?       MinPrice       { get; set; }
        public decimal?       MaxPrice       { get; set; }
        public string?        SortBy         { get; set; }
        public int            TotalCount     { get; set; }
        public int            Page           { get; set; } = 1;
        public int            PageSize       { get; set; } = 12;
        public int            TotalPages     => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ProductDetailViewModel
    {
        public Product        Product       { get; set; } = null!;
        public List<Review>   Reviews       { get; set; } = new();
        public List<Product>  RelatedItems  { get; set; } = new();
        public bool           IsInWishlist  { get; set; }
        public double         AvgRating     { get; set; }
        public bool           UserReviewed  { get; set; }
    }

    // ── Cart / Checkout ──────────────────────────────────────────────────────────
    //public class CartViewModel
    //{
    //    public List<CartItemViewModel> Items      { get; set; } = new();
    //    public decimal                 SubTotal   => Items.Sum(i => i.SubTotal);
    //    public decimal                 ShippingFee { get; set; } = 60;
    //    public decimal                 Total      => SubTotal + ShippingFee;
    //}


    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public string DeliveryZone { get; set; } = "ঢাকার ভিতর";
        public decimal SubTotal => Items.Sum(i => i.SubTotal);
        public decimal ShippingFee => DeliveryZone == "ঢাকার বাইরে" ? 120 : 60;
        public decimal Total => SubTotal + ShippingFee;
    }

    //public class CartItemViewModel
    //{
    //    public int     CartId    { get; set; }
    //    public int     ProductId { get; set; }
    //    public string  Name      { get; set; } = string.Empty;
    //    public string? ImageUrl  { get; set; }
    //    public decimal Price     { get; set; }
    //    public int     Quantity  { get; set; }
    //    public int     Stock     { get; set; }
    //    public decimal SubTotal  => Price * Quantity;
    //}


    public class CartItemViewModel
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int? ProductSizeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SizeName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public decimal SubTotal => Price * Quantity;
    }

    //public class CheckoutViewModel
    //{
    //    [Required, Display(Name = "Shipping Address")]
    //    public string ShippingAddress { get; set; } = string.Empty;

    //    [Required, Phone]
    //    public string Phone { get; set; } = string.Empty;

    //    [Display(Name = "Payment Method")]
    //    public string PaymentMethod { get; set; } = "Cash on Delivery";

    //    public string? Notes { get; set; }

    //    public CartViewModel Cart { get; set; } = new();
    //}

    public class ComboCreateViewModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal ComboPrice { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public IFormFile? BannerImage { get; set; }

        // Selected product IDs + quantities
        public List<ComboProductEntry> Products { get; set; } = new();
    }

    public class ComboProductEntry
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // ── Homepage Section Create ViewModel ────────────────────────────────────────
    public class HomepageSectionCreateViewModel
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string SectionType { get; set; } = "category"; // category|combo|product|banner

        public string? SubTitle { get; set; }
        public string? BgColor { get; set; } = "#ffffff";
        public string? TextColor { get; set; } = "#000000";
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public IFormFile? BannerImage { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();
        public List<int> SelectedProductIds { get; set; } = new();
    }


    public class CheckoutViewModel
    {
        [Required, Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        // ── নতুন ──────────────────────────────────────────────────────────────────
        [Required, Display(Name = "Delivery Zone")]
        public string DeliveryZone { get; set; } = "ঢাকার ভিতর";

        public string? Notes { get; set; }
        public CartViewModel Cart { get; set; } = new();
    }




    // ── Review ───────────────────────────────────────────────────────────────────
    public class ReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    // ── Admin ViewModels ─────────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int     TotalProducts  { get; set; }
        public int     TotalOrders    { get; set; }
        public int     TotalUsers     { get; set; }
        public int     PendingOrders  { get; set; }
        public decimal TotalRevenue   { get; set; }
        public List<Order>   RecentOrders  { get; set; } = new();
        public List<Product> LowStockItems { get; set; } = new();
    }

    //public class ProductCreateViewModel
    //{
    //    [Required, MaxLength(200)]
    //    public string Name { get; set; } = string.Empty;

    //    [Required]
    //    public int CategoryId { get; set; }

    //    [Required, Column]
    //    public decimal Price { get; set; }

    //    public decimal? DiscountPrice { get; set; }

    //    [Required]
    //    public int Stock { get; set; }

    //    public string? Description { get; set; }
    //    public bool IsActive { get; set; } = true;

    //    public List<IFormFile>? Images { get; set; }
    //    public List<int>?       RemoveImageIds { get; set; }
    //}


    public class ProductCreateViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal ProductVat { get; set; } = 0;
        public bool IsDiscountFixed { get; set; } = true;
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        [Required]
        public int Stock { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public List<IFormFile>? Images { get; set; }
        public List<int>? RemoveImageIds { get; set; }

        // Size entries
        public List<ProductSizeEntry> Sizes { get; set; } = new();
    }


    public class ProductSizeEntry
    {
        public int? Id { get; set; } // edit এর জন্য
        public string SizeName { get; set; } = string.Empty;
        public string? SizeCode { get; set; }
        public decimal SalesPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRemove { get; set; } = false;
    }


    public class OrderAdminViewModel
    {
        public Order Order { get; set; } = null!;
        public List<string> StatusOptions { get; set; } = new()
        {
            "Pending", "Processing", "Shipped", "Completed", "Cancelled"
        };
    }





}

// Dummy attribute for ViewModel (no EF needed here)
namespace System.ComponentModel.DataAnnotations
{
    public class ColumnAttribute : Attribute { }
}
