using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Interfaces;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public ChatController(
            IChatRepository chatRepo,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _chatRepo = chatRepo;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        private string UserId => _userManager.GetUserId(User)!;

        // GET /Chat — User chat page
        public async Task<IActionResult> Index()
        {
            var session = await _chatRepo.GetOrCreateSessionAsync(UserId);
            var messages = await _chatRepo.GetMessagesAsync(UserId);

            await _chatRepo.ResetUnreadCountAsync(session);
            await _chatRepo.MarkMessagesAsReadAsync(UserId);

            ViewBag.CurrentUser = await _userManager.GetUserAsync(User);

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.AdminPhoto = adminUsers.FirstOrDefault()?.ProfilePhoto;

            return View(messages);
        }

        // GET /Chat/UnreadCount — AJAX poll
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _chatRepo.GetUnreadCountAsync(UserId);
            return Json(new { count });
        }

        // POST /Chat/MarkRead
        [HttpPost]
        public async Task<IActionResult> MarkRead()
        {
            await _chatRepo.MarkMessagesAsReadAsync(UserId);
            return Json(new { success = true });
        }

        // POST /Chat/SendMessage
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return Json(new { success = false, message = "Please write a message." });

            var message = await _chatRepo.AddUserMessageAsync(UserId, messageText);

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _notificationService.SendNewChatMessageNotificationAsync(
                    currentUser!.FullName, messageText);
            }
            catch
            {
            }

            return Json(new
            {
                success = true,
                messageId = message.Id,
                sentAt = message.SentAt.ToString("hh:mm tt")
            });
        }
    }
}