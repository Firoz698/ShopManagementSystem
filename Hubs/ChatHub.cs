using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        // Online users track করার জন্য static dictionary
        private static readonly Dictionary<string, string> _onlineUsers = new();

        public ChatHub(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── Connection ────────────────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            _onlineUsers[user.Id] = Context.ConnectionId;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                // Admin সব user এর group এ join করবে
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                // সব unread count পাঠাও
                await Clients.Caller.SendAsync("AdminConnected");
            }
            else
            {
                // User তার নিজের group এ join করবে
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{user.Id}");

                // ChatSession নিশ্চিত করো
                var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (session == null)
                {
                    session = new ChatSession { UserId = user.Id };
                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();
                }

                // Admin কে জানাও user online হয়েছে
                await Clients.Group("Admins").SendAsync("UserOnline", new
                {
                    userId = user.Id,
                    userName = user.FullName,
                    email = user.Email
                });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                _onlineUsers.Remove(user.Id);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    await Clients.Group("Admins").SendAsync("UserOffline", user.Id);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ── User → Admin মেসেজ পাঠানো ────────────────────────────────────────
        public async Task SendMessageToAdmin(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null || string.IsNullOrWhiteSpace(message)) return;

            // Session update
            var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (session == null)
            {
                session = new ChatSession { UserId = user.Id };
                _db.ChatSessions.Add(session);
            }
            session.LastMessageAt = DateTime.Now;
            session.UnreadCount++;

            // Message save
            var chatMsg = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = "admin",
                Message = message.Trim(),
                IsFromAdmin = false,
                SentAt = DateTime.Now
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            var payload = new
            {
                id = chatMsg.Id,
                message = chatMsg.Message,
                senderName = user.FullName,
                senderId = user.Id,
                senderEmail = user.Email,
                sentAt = chatMsg.SentAt.ToString("hh:mm tt"),
                isFromAdmin = false
            };

            // Admin কে পাঠাও
            await Clients.Group("Admins").SendAsync("ReceiveMessage", payload);

            // নিজেকেও confirm পাঠাও
            await Clients.Caller.SendAsync("MessageSent", payload);
        }

        // ── Admin → User মেসেজ পাঠানো ────────────────────────────────────────
        public async Task SendMessageToUser(string targetUserId, string message)
        {
            var admin = await _userManager.GetUserAsync(Context.User!);
            if (admin == null || string.IsNullOrWhiteSpace(message)) return;

            var isAdmin = await _userManager.IsInRoleAsync(admin, "Admin");
            if (!isAdmin) return;

            // Session এর unread reset
            var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == targetUserId);
            if (session != null)
            {
                session.LastMessageAt = DateTime.Now;
            }

            // Message save
            var chatMsg = new ChatMessage
            {
                SenderId = admin.Id,
                ReceiverId = targetUserId,
                Message = message.Trim(),
                IsFromAdmin = true,
                SentAt = DateTime.Now
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            var payload = new
            {
                id = chatMsg.Id,
                message = chatMsg.Message,
                senderName = "Admin",
                senderId = admin.Id,
                sentAt = chatMsg.SentAt.ToString("hh:mm tt"),
                isFromAdmin = true
            };

            // User কে পাঠাও
            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveMessage", payload);

            // Admin নিজেও দেখবে
            await Clients.Caller.SendAsync("MessageSent", new
            {
                payload,
                targetUserId
            });
        }

        // ── Messages পড়া হয়েছে mark করা ─────────────────────────────────────
        public async Task MarkAsRead(string userId)
        {
            var admin = await _userManager.GetUserAsync(Context.User!);
            if (admin == null) return;

            var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == userId);
            if (session != null)
            {
                session.UnreadCount = 0;
                await _db.SaveChangesAsync();
            }

            await _db.ChatMessages
                .Where(m => m.SenderId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        // ── চেক করো user online কিনা ─────────────────────────────────────────
        public bool IsUserOnline(string userId) => _onlineUsers.ContainsKey(userId);
    }
}
