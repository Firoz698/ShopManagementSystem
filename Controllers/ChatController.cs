using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IChatRepository chatRepo, UserManager<ApplicationUser> userManager)
        {
            _chatRepo = chatRepo;
            _userManager = userManager;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // GET /Chat — User chat page
        public async Task<IActionResult> Index()
        {
            // Session নিশ্চিত করো
            var session = await _chatRepo.GetOrCreateSessionAsync(UserId);

            // পুরনো messages লোড করো
            var messages = await _chatRepo.GetMessagesAsync(UserId);

            // Unread reset
            await _chatRepo.ResetUnreadCountAsync(session);
            await _chatRepo.MarkMessagesAsReadAsync(UserId);

            ViewBag.CurrentUser = await _userManager.GetUserAsync(User);
            return View(messages);
        }

        // GET /Chat/UnreadCount — AJAX poll
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _chatRepo.GetUnreadCountAsync(UserId);
            return Json(new { count });
        }
    }
}