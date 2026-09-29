using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;

namespace ShopManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class ChatController : Controller
    {
        private readonly IAdminChatRepository _chatRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionChecker _permissionChecker;

        public ChatController(
            IAdminChatRepository chatRepo,
            UserManager<ApplicationUser> userManager,
            IPermissionChecker permissionChecker)
        {
            _chatRepo = chatRepo;
            _userManager = userManager;
            _permissionChecker = permissionChecker;
        }

        // GET /Admin/Chat - show all sessions
        public async Task<IActionResult> Index()
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Chat");
            if (!perm.CanView)
                return Forbid();

            var sessions = await _chatRepo.GetAllSessionsAsync();
            ViewBag.TotalUnread = sessions.Sum(s => s.UnreadCount);
            return View(sessions);
        }

        // GET /Admin/Chat/Conversation/userId (full page)
        public async Task<IActionResult> Conversation(string userId)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Chat");
            if (!perm.CanView)
                return Forbid();

            if (string.IsNullOrWhiteSpace(userId))
                return NotFound();

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
        // AJAX endpoint to load the right panel without page reload
        [HttpGet]
        public async Task<IActionResult> ConversationPartial(string userId)
        {
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Chat");
            if (!perm.CanView)
                return Forbid();

            if (string.IsNullOrWhiteSpace(userId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var messages = await _chatRepo.GetConversationMessagesAsync(userId);

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
            var perm = await _permissionChecker.GetPermissionsAsync(User, "Chat");
            if (!perm.CanView)
                return Forbid();

            var count = await _chatRepo.GetTotalUnreadCountAsync();
            return Json(new { count });
        }
    }
}