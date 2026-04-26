using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ReturnController : Controller
    {
        private readonly IReturnRepository _returnRepo;

        // ── Status Flow ──────────────────────────────────────────────────────────
        // Pending → Approved → Refunded
        // Pending → Rejected
        private static readonly Dictionary<string, string[]> _nextStatuses = new()
        {
            { "Pending",  new[] { "Approved", "Rejected" } },
            { "Approved", new[] { "Refunded" } },
            { "Rejected", Array.Empty<string>() },
            { "Refunded", Array.Empty<string>() }
        };

        public ReturnController(IReturnRepository returnRepo)
        {
            _returnRepo = returnRepo;
        }

        // GET /Admin/Return
        public async Task<IActionResult> Index(string? status)
        {
            var returns = await _returnRepo.GetAllAsync(status);

            ViewBag.Status = status;
            ViewBag.StatusList = new[] { "Pending", "Approved", "Rejected", "Refunded" };
            ViewBag.PendingCount = await _returnRepo.GetPendingCountAsync();

            return View(returns);
        }

        // GET /Admin/Return/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ret = await _returnRepo.GetDetailAsync(id);
            if (ret == null) return NotFound();

            ViewBag.NextStatuses = _nextStatuses.GetValueOrDefault(ret.Status, Array.Empty<string>());
            return View(ret);
        }

        // POST /Admin/Return/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNote)
        {
            var ret = await _returnRepo.GetWithOrderAsync(id);
            if (ret == null)
            {
                TempData["Error"] = "রিটার্ন রিকোয়েস্ট পাওয়া যায়নি।";
                return RedirectToAction("Index");
            }

            // Valid transition চেক
            var allowed = _nextStatuses.GetValueOrDefault(ret.Status, Array.Empty<string>());
            if (!allowed.Contains(status))
            {
                TempData["Error"] = $"'{ret.Status}' থেকে '{status}' তে পরিবর্তন করা যাবে না।";
                return RedirectToAction("Details", new { id });
            }

            await _returnRepo.UpdateStatusAsync(ret, status, adminNote);

            TempData["Success"] = $"রিটার্ন #{id} — {status} করা হয়েছে।";
            return RedirectToAction("Details", new { id });
        }
    }
}