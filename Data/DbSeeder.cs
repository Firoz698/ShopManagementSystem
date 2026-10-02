using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedMenusAsync(ApplicationDbContext db)
        {
            if (await db.Menus.AnyAsync())
            {
                // Ensure Delivery menu exists even if database was previously seeded
                if (!await db.Menus.AnyAsync(m => m.Controller == "Delivery"))
                {
                    var salesParent = await db.Menus.FirstOrDefaultAsync(m => m.Title == "Sales & Support" && m.ParentId == null)
                                     ?? await db.Menus.FirstOrDefaultAsync(m => m.ParentId == null);

                    if (salesParent != null)
                    {
                        db.Menus.Add(new Menu
                        {
                            Title = "Delivery & Tracking",
                            Icon = "bi bi-truck",
                            Controller = "Delivery",
                            Action = "Index",
                            Area = "Admin",
                            ParentId = salesParent.Id,
                            SortOrder = 2,
                            IsActive = true
                        });
                        await db.SaveChangesAsync();
                    }
                }
                return;
            }

            // 1. Main
            var headerMain = new Menu
            {
                Title = "Main",
                Icon = null,
                Controller = null,
                Action = null,
                Area = "Admin",
                SortOrder = 1,
                IsActive = true
            };
            db.Menus.Add(headerMain);
            await db.SaveChangesAsync();

            db.Menus.AddRange(
                new Menu { Title = "Dashboard", Icon = "bi bi-speedometer2", Controller = "Dashboard", Action = "Index", Area = "Admin", ParentId = headerMain.Id, SortOrder = 1, IsActive = true }
            );

            // 2. Catalog
            var headerCatalog = new Menu
            {
                Title = "Catalog",
                Icon = null,
                Controller = null,
                Action = null,
                Area = "Admin",
                SortOrder = 2,
                IsActive = true
            };
            db.Menus.Add(headerCatalog);
            await db.SaveChangesAsync();

            db.Menus.AddRange(
                new Menu { Title = "Products", Icon = "bi bi-box-seam", Controller = "Product", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "Categories", Icon = "bi bi-tag", Controller = "Category", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "Sliders", Icon = "bi bi-images", Controller = "Slider", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 3, IsActive = true },
                new Menu { Title = "Combo Offers", Icon = "bi bi-gift", Controller = "Combo", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 4, IsActive = true },
                new Menu { Title = "Homepage Sections", Icon = "bi bi-layout-text-window", Controller = "HomepageSection", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 5, IsActive = true }
            );

            // 3. Sales & Support
            var headerSales = new Menu
            {
                Title = "Sales & Support",
                Icon = null,
                Controller = null,
                Action = null,
                Area = "Admin",
                SortOrder = 3,
                IsActive = true
            };
            db.Menus.Add(headerSales);
            await db.SaveChangesAsync();

            db.Menus.AddRange(
                new Menu { Title = "Orders", Icon = "bi bi-receipt", Controller = "Order", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "Delivery & Tracking", Icon = "bi bi-truck", Controller = "Delivery", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "Return Requests", Icon = "bi bi-arrow-return-left", Controller = "Return", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 3, IsActive = true },
                new Menu { Title = "Chat Support", Icon = "bi bi-chat-dots", Controller = "Chat", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 4, IsActive = true }
            );

            // 4. Users & Permissions
            var headerUsers = new Menu
            {
                Title = "Users & Permissions",
                Icon = null,
                Controller = null,
                Action = null,
                Area = "Admin",
                SortOrder = 4,
                IsActive = true
            };
            db.Menus.Add(headerUsers);
            await db.SaveChangesAsync();

            db.Menus.AddRange(
                new Menu { Title = "Users", Icon = "bi bi-people", Controller = "User", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "Employees", Icon = "bi bi-person-badge", Controller = "Employee", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "Role Management", Icon = "bi bi-shield-check", Controller = "Role", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 3, IsActive = true },
                new Menu { Title = "Menu Management", Icon = "bi bi-list-nested", Controller = "Menu", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 4, IsActive = true }
            );

            // 5. Settings
            var headerSettings = new Menu
            {
                Title = "Settings",
                Icon = null,
                Controller = null,
                Action = null,
                Area = "Admin",
                SortOrder = 5,
                IsActive = true
            };
            db.Menus.Add(headerSettings);
            await db.SaveChangesAsync();

            db.Menus.AddRange(
                new Menu { Title = "Notification Settings", Icon = "bi bi-bell", Controller = "NotificationSetting", Action = "Index", Area = "Admin", ParentId = headerSettings.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "Payment Methods", Icon = "bi bi-credit-card-2-front", Controller = "PaymentMethod", Action = "Index", Area = "Admin", ParentId = headerSettings.Id, SortOrder = 2, IsActive = true }
            );

            await db.SaveChangesAsync();
        }
    }
}

