using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedMenusAsync(ApplicationDbContext db)
        {
            if (await db.Menus.AnyAsync()) return;

            // 1. প্রধান
            var headerMain = new Menu
            {
                Title = "প্রধান",
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
                new Menu { Title = "ড্যাশবোর্ড", Icon = "bi bi-speedometer2", Controller = "Dashboard", Action = "Index", Area = "Admin", ParentId = headerMain.Id, SortOrder = 1, IsActive = true }
            );

            // 2. ক্যাটালগ
            var headerCatalog = new Menu
            {
                Title = "ক্যাটালগ",
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
                new Menu { Title = "প্রোডাক্ট", Icon = "bi bi-box-seam", Controller = "Product", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "ক্যাটাগরি", Icon = "bi bi-tag", Controller = "Category", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "স্লাইডার", Icon = "bi bi-images", Controller = "Slider", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 3, IsActive = true },
                new Menu { Title = "কম্বো অফার", Icon = "bi bi-gift", Controller = "Combo", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 4, IsActive = true },
                new Menu { Title = "হোমপেজ সেকশন", Icon = "bi bi-layout-text-window", Controller = "HomepageSection", Action = "Index", Area = "Admin", ParentId = headerCatalog.Id, SortOrder = 5, IsActive = true }
            );

            // 3. বিক্রয় ও গ্রাহক
            var headerSales = new Menu
            {
                Title = "বিক্রয় ও গ্রাহক",
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
                new Menu { Title = "অর্ডার", Icon = "bi bi-receipt", Controller = "Order", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "রিটার্ন রিকোয়েস্ট", Icon = "bi bi-arrow-return-left", Controller = "Return", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "চ্যাট সাপোর্ট", Icon = "bi bi-chat-dots", Controller = "Chat", Action = "Index", Area = "Admin", ParentId = headerSales.Id, SortOrder = 3, IsActive = true }
            );

            // 4. ব্যবহারকারী ও পারমিশন
            var headerUsers = new Menu
            {
                Title = "ব্যবহারকারী ও পারমিশন",
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
                new Menu { Title = "ইউজার", Icon = "bi bi-people", Controller = "User", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "এমপ্লয়ী", Icon = "bi bi-person-badge", Controller = "Employee", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 2, IsActive = true },
                new Menu { Title = "রোল ম্যানেজমেন্ট", Icon = "bi bi-shield-check", Controller = "Role", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 3, IsActive = true },
                new Menu { Title = "মেনু ম্যানেজমেন্ট", Icon = "bi bi-list-nested", Controller = "Menu", Action = "Index", Area = "Admin", ParentId = headerUsers.Id, SortOrder = 4, IsActive = true }
            );

            // 5. সেটিংস
            var headerSettings = new Menu
            {
                Title = "সেটিংস",
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
                new Menu { Title = "নোটিফিকেশন সেটিংস", Icon = "bi bi-bell", Controller = "NotificationSetting", Action = "Index", Area = "Admin", ParentId = headerSettings.Id, SortOrder = 1, IsActive = true },
                new Menu { Title = "পেমেন্ট মেথড", Icon = "bi bi-credit-card-2-front", Controller = "PaymentMethod", Action = "Index", Area = "Admin", ParentId = headerSettings.Id, SortOrder = 2, IsActive = true }
            );

            await db.SaveChangesAsync();
        }
    }
}

