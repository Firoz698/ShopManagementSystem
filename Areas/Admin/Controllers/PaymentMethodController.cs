using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class PaymentMethodController : Controller
    {
        private readonly IAdminPaymentMethodRepository _repo;

        public PaymentMethodController(IAdminPaymentMethodRepository repo)
        {
            _repo = repo;
        }

        // GET /Admin/PaymentMethod
        public async Task<IActionResult> Index()
        {
            var methods = await _repo.GetAllAsync();
            return View(methods);
        }

        // GET /Admin/PaymentMethod/Create
        public IActionResult Create()
        {
            return View(new PaymentMethodSetting());
        }

        // POST /Admin/PaymentMethod/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethodSetting model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _repo.CreateAsync(model);
            TempData["Success"] = "পেমেন্ট মেথড যোগ করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // GET /Admin/PaymentMethod/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var method = await _repo.GetByIdAsync(id);
            if (method == null) return NotFound();
            return View(method);
        }

        // POST /Admin/PaymentMethod/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentMethodSetting model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _repo.UpdateAsync(model);
            TempData["Success"] = "পেমেন্ট মেথড আপডেট করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/PaymentMethod/ToggleActive/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _repo.ToggleActiveAsync(id);
            TempData["Success"] = "স্ট্যাটাস পরিবর্তন করা হয়েছে।";
            return RedirectToAction("Index");
        }

        // POST /Admin/PaymentMethod/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "পেমেন্ট মেথড ডিলিট করা হয়েছে।";
            return RedirectToAction("Index");
        }
    }
}