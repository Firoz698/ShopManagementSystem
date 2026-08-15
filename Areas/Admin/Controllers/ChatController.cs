using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ChatController : Controller
    {
        private readonly IAdminChatRepository _chatRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IAdminChatRepository chatRepo, UserManager<ApplicationUser> userManager)
        {
            _chatRepo = chatRepo;
            _userManager = userManager;
        }

        // GET /Admin/Chat — সব sessions দেখাবে
        public async Task<IActionResult> Index()
        {
            var sessions = await _chatRepo.GetAllSessionsAsync();
            ViewBag.TotalUnread = sessions.Sum(s => s.UnreadCount);
            return View(sessions);
        }

        // GET /Admin/Chat/Conversation/userId
        // (সরাসরি লিংক/ডিপ-লিংক এর জন্য রাখা হলো — full page)
        public async Task<IActionResult> Conversation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var messages = await _chatRepo.GetConversationMessagesAsync(userId);

            // Mark as read
            var session = await _chatRepo.GetSessionByUserIdAsync(userId);
            if (session != null)
                await _chatRepo.ResetSessionUnreadCountAsync(session);

            await _chatRepo.MarkUserMessagesAsReadAsync(userId);

            ViewBag.TargetUser = user;
            ViewBag.TargetUserId = userId;
            return View(messages);
        }

        // GET /Admin/Chat/ConversationPartial?userId=...
        // ── AJAX দিয়ে ডান পাশের প্যানেল লোড করার জন্য (page reload হয় না) ──
        [HttpGet]
        public async Task<IActionResult> ConversationPartial(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var messages = await _chatRepo.GetConversationMessagesAsync(userId);

            // Mark as read
            var session = await _chatRepo.GetSessionByUserIdAsync(userId);
            if (session != null)
                await _chatRepo.ResetSessionUnreadCountAsync(session);

            await _chatRepo.MarkUserMessagesAsReadAsync(userId);

            ViewBag.TargetUser = user;
            ViewBag.TargetUserId = userId;
            return PartialView("_ChatConversation", messages);
        }

        // GET /Admin/Chat/UnreadTotal — AJAX poll
        [HttpGet]
        public async Task<IActionResult> UnreadTotal()
        {
            var count = await _chatRepo.GetTotalUnreadCountAsync();
            return Json(new { count });
        }
    }
}