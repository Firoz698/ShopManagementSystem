using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")] // শুধু Admin — Employee এখানে ঢুকতে পারবে না
    public class MenuController : Controller
    {
        private readonly IAdminMenuRepository _repo;
        public MenuController(IAdminMenuRepository repo) => _repo = repo;

        public async Task<IActionResult> Index()
        {
            var menus = await _repo.GetAllAsync();
            return View(menus);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Headers = await _repo.GetHeaderOptionsAsync();
            return View(new Menu());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Headers = await _repo.GetHeaderOptionsAsync();
                return View(model);
            }

            await _repo.CreateAsync(model);
            TempData["Success"] = "মেনু তৈরি হয়েছে।";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _repo.GetByIdAsync(id);
            if (menu == null) return NotFound();

            ViewBag.Headers = (await _repo.GetHeaderOptionsAsync()).Where(h => h.Id != id).ToList();
            return View(menu);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Menu model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Headers = (await _repo.GetHeaderOptionsAsync()).Where(h => h.Id != model.Id).ToList();
                return View(model);
            }

            await _repo.UpdateAsync(model);
            TempData["Success"] = "মেনু আপডেট হয়েছে।";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "মেনু ডিলিট হয়েছে (চাইল্ড থাকলে ডিলিট হবে না)।";
            return RedirectToAction("Index");
        }
    }
}