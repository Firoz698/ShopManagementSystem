using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category>     Categories   { get; set; }
        public DbSet<Product>      Products     { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Slider>       Sliders      { get; set; }
        public DbSet<Cart>         Carts        { get; set; }
        public DbSet<Wishlist>     Wishlists    { get; set; }
        public DbSet<Order>        Orders       { get; set; }
        public DbSet<OrderDetail>  OrderDetails { get; set; }
        public DbSet<Review>       Reviews      { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }

        public DbSet<ProductSize> ProductSizes { get; set; }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ReturnRequest>()
                .HasIndex(r => new { r.OrderId, r.ProductId, r.UserId });

            builder.Entity<ChatMessage>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ChatSession>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatSession>()
                .HasMany(cs => cs.Messages)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatSession>()
                .HasIndex(cs => cs.UserId)
                .IsUnique();


            builder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Sizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasOne(c => c.ProductSize)
                .WithMany()
                .HasForeignKey(c => c.ProductSizeId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.Entity<PaymentTransaction>()
                .HasOne(p => p.Order)
                .WithOne()
                .HasForeignKey<PaymentTransaction>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            builder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            builder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();

            builder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique();

            // Cascade rules
            builder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics",  Slug = "electronics",  IsActive = true, CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Clothing",     Slug = "clothing",     IsActive = true, CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Books",        Slug = "books",        IsActive = true, CreatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Home & Garden",Slug = "home-garden",  IsActive = true, CreatedAt = DateTime.Now }
            );

            builder.Entity<Slider>().HasData(
                new Slider { Id = 1, ImageUrl = "/images/uploads/slider1.jpg", Title = "Welcome to ShopManagementSystem", SubTitle = "Best products at best prices", ButtonText = "Shop Now", ButtonUrl = "/Product", IsActive = true, SortOrder = 1, CreatedAt = DateTime.Now },
                new Slider { Id = 2, ImageUrl = "/images/uploads/slider2.jpg", Title = "Summer Sale — Up to 50% Off", SubTitle = "Limited time offer", ButtonText = "View Deals", ButtonUrl = "/Product", IsActive = true, SortOrder = 2, CreatedAt = DateTime.Now }
            );
        }
    }
}
