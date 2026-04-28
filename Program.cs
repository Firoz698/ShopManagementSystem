using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Implementations;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Generic.Repository.Implementations;
using ShopManagementSystem.Generic.Repository.Interfaces;
using ShopManagementSystem.Implementations;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Implementations;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
    options.Password.RequiredLength         = 6;
    options.SignIn.RequireConfirmedEmail     = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.AccessDeniedPath  = "/Account/AccessDenied";
    options.ExpireTimeSpan    = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Generic + Specific ───────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISslCommerzService, SslCommerzService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();

//----For Admin
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAdminChatRepository, AdminChatRepository>();
builder.Services.AddScoped<IAdminProductRepository, AdminProductRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IReturnRepository, ReturnRepository>();
builder.Services.AddScoped<ISliderRepository, SliderRepository>();
builder.Services.AddScoped<IAdminOrderRepository, AdminOrderRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();





builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Seed roles and admin ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Default admin
    const string adminEmail = "admin@shopmanagement.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            FullName = "Administrator",
            UserName = adminEmail,
            Email    = adminEmail,
            PhoneNumber = "01700000000",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@1234");
        if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// ── Middleware ───────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ShopManagementSystem.Hubs.ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
