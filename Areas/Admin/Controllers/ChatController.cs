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

        // GET /Admin/Chat/UnreadTotal — AJAX poll
        [HttpGet]
        public async Task<IActionResult> UnreadTotal()
        {
            var count = await _chatRepo.GetTotalUnreadCountAsync();
            return Json(new { count });
        }
    }
}