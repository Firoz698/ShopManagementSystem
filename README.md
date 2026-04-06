# ShopManagementSystem
### Developed by **Md Firoz**

ASP.NET Core 8 MVC + SQL Server দিয়ে তৈরি একটি সম্পূর্ণ ই-কমার্স ম্যানেজমেন্ট সিস্টেম।

---

## ✅ ফিচার সমূহ

### 🛍️ ইউজার সাইড
- ডায়নামিক হোম স্লাইডার (অ্যাডমিন থেকে কন্ট্রোলযোগ্য)
- ক্যাটাগরি ব্রাউজিং ও ফিল্টারিং
- প্রোডাক্ট সার্চ (নাম, বিবরণ)
- মূল্য ও ক্যাটাগরি অনুযায়ী ফিল্টার
- প্রোডাক্ট ডিটেইল পেজ (মাল্টিপল ছবি, রেটিং, রিভিউ)
- কার্ট (যোগ, পরিমাণ আপডেট, সরান)
- চেকআউট (ঠিকানা, ফোন, পেমেন্ট পদ্ধতি)
- উইশলিস্ট (যোগ/সরান)
- রিভিউ ও স্টার রেটিং সিস্টেম
- অর্ডার হিস্ট্রি
- লগইন / রেজিস্টার

### 🔧 অ্যাডমিন প্যানেল (/Admin/Dashboard)
- ড্যাশবোর্ড (মোট প্রোডাক্ট, অর্ডার, ইউজার, আয়)
- প্রোডাক্ট CRUD + মাল্টিপল ইমেজ আপলোড
- প্রোডাক্ট Active/Inactive টগল + স্টক ম্যানেজমেন্ট
- ক্যাটাগরি CRUD + ইমেজ
- ডায়নামিক স্লাইডার ম্যানেজমেন্ট (টাইটেল, বাটন, সর্ট অর্ডার)
- অর্ডার ম্যানেজমেন্ট + স্ট্যাটাস আপডেট
- ইউজার ম্যানেজমেন্ট + রোল পরিবর্তন

---

## 🚀 প্রজেক্ট সেটআপ

### পূর্বশর্ত
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB বা Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) বা VS Code

### ধাপ ১ — প্রজেক্ট ক্লোন/ডাউনলোড করুন

### ধাপ ২ — Connection String সেট করুন
`appsettings.json` ফাইলে আপনার SQL Server connection string দিন:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShopManagementSystemDb;Trusted_Connection=True;"
  }
}
```

### ধাপ ৩ — Database Migration চালান

```bash
# Package Manager Console (Visual Studio) থেকে:
Add-Migration InitialCreate
Update-Database

# অথবা .NET CLI থেকে:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> **বিকল্প:** `Database_Script.sql` ফাইলটি SQL Server Management Studio-তে রান করুন।

### ধাপ ৪ — প্রজেক্ট রান করুন

```bash
dotnet run
```

অথবা Visual Studio-তে F5 চাপুন।

---

## 🔑 ডিফল্ট অ্যাডমিন অ্যাকাউন্ট

| ইমেইল | পাসওয়ার্ড |
|-------|-----------|
| `admin@shopmanagement.com` | `Admin@1234` |

অ্যাডমিন প্যানেল: `/Admin/Dashboard`

---

## 📁 প্রজেক্ট স্ট্রাকচার

```
ShopManagementSystem/
├── Areas/
│   └── Admin/
│       ├── Controllers/          ← অ্যাডমিন কন্ট্রোলার
│       │   ├── DashboardController.cs
│       │   ├── ProductController.cs
│       │   ├── CategoryController.cs
│       │   ├── SliderController.cs
│       │   ├── OrderController.cs
│       │   └── UserController.cs
│       └── Views/Admin/          ← অ্যাডমিন ভিউ
├── Controllers/                  ← ইউজার কন্ট্রোলার
│   ├── HomeController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   ├── WishlistController.cs
│   └── AccountController.cs
├── Data/
│   └── ApplicationDbContext.cs   ← EF DbContext + Seed Data
├── Models/
│   └── Models.cs                 ← সব Entity Model
├── ViewModels/
│   └── ViewModels.cs             ← সব ViewModel
├── Services/
│   └── ImageService.cs           ← ইমেজ আপলোড সার্ভিস
├── Views/                        ← ইউজার-সাইড Views
├── wwwroot/
│   ├── css/site.css
│   └── images/uploads/           ← আপলোড করা ছবি
├── Program.cs                    ← DI + Middleware + Seed Roles
├── appsettings.json
└── Database_Script.sql           ← ম্যানুয়াল DB স্ক্রিপ্ট
```

---

## 🗄️ ডাটাবেজ টেবিল সমূহ

| টেবিল | বিবরণ |
|-------|-------|
| `AspNetUsers` | ইউজার অ্যাকাউন্ট (ASP.NET Identity) |
| `AspNetRoles` | ভূমিকা (Admin, User) |
| `Categories` | পণ্যের ক্যাটাগরি |
| `Products` | পণ্য তথ্য + স্টক |
| `ProductImages` | পণ্যের একাধিক ছবি |
| `Sliders` | হোমপেজ স্লাইডার |
| `Carts` | কার্ট আইটেম |
| `Wishlists` | উইশলিস্ট |
| `Orders` | অর্ডার হেডার |
| `OrderDetails` | অর্ডার লাইন আইটেম |
| `Reviews` | পণ্য রিভিউ ও রেটিং |

---

## 🛠️ ব্যবহৃত প্রযুক্তি

- **Backend:** ASP.NET Core 8 MVC
- **ORM:** Entity Framework Core 8 (Code First)
- **Database:** SQL Server / LocalDB
- **Auth:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5.3, Bootstrap Icons
- **Image Storage:** Server wwwroot filesystem

---

*Developed with ❤️ by **Md Firoz***
