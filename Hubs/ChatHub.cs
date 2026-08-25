using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Services;
using System.Collections.Concurrent;

namespace ShopManagementSystem.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const int MaxMessageLength = 1000;

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionChecker _permissionChecker;

        // userId -> ওই ইউজারের সব active connectionId (multi-tab/device safe)
        private static readonly ConcurrentDictionary<string, HashSet<string>> _onlineUsers = new();
        private static readonly object _onlineLock = new();

        public ChatHub(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IPermissionChecker permissionChecker)
        {
            _db = db;
            _userManager = userManager;
            _permissionChecker = permissionChecker;
        }

        // ── Connection ────────────────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            AddConnection(user.Id, Context.ConnectionId);

            // Role-এর বদলে actual "Chat" permission দিয়ে চেক — non-Admin role হলেও 
            // menu permission থাকলে Admins গ্রুপে ঢুকবে
            var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
            if (perm.CanView)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                await Clients.Caller.SendAsync("AdminConnected");
            }
            else
            {
                // সাধারণ কাস্টমার হিসেবে ট্রিট করো
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{user.Id}");

                var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (session == null)
                {
                    session = new ChatSession { UserId = user.Id };
                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();
                }

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
                var stillOnline = RemoveConnection(user.Id, Context.ConnectionId);

                // শুধু তখনই "Offline" পাঠাও যখন ইউজারের আর কোনো active connection নেই
                if (!stillOnline)
                {
                    var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
                    if (!perm.CanView)
                    {
                        await Clients.Group("Admins").SendAsync("UserOffline", user.Id);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ── User → Admin মেসেজ পাঠানো ────────────────────────────────────────
        public async Task SendMessageToAdmin(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            message = (message ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(message) || message.Length > MaxMessageLength) return;

            var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (session == null)
            {
                session = new ChatSession { UserId = user.Id };
                _db.ChatSessions.Add(session);
            }
            session.LastMessageAt = DateTime.Now;
            session.UnreadCount++;

            var chatMsg = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = "admin",
                Message = message,
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

            await Clients.Group("Admins").SendAsync("ReceiveMessage", payload);
            await Clients.Caller.SendAsync("MessageSent", payload);
        }

        // ── Admin → User মেসেজ পাঠানো ────────────────────────────────────────
        public async Task SendMessageToUser(string targetUserId, string message)
        {
            var admin = await _userManager.GetUserAsync(Context.User!);
            if (admin == null || string.IsNullOrWhiteSpace(targetUserId)) return;

            message = (message ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(message) || message.Length > MaxMessageLength) return;

            // ✅ Role-এর বদলে actual permission চেক — CanCreate না থাকলে reject
            var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
            if (!perm.CanCreate)
            {
                await Clients.Caller.SendAsync("Error", "মেসেজ পাঠানোর অনুমতি আপনার নেই।");
                return;
            }

            // Session না থাকলে তৈরি করো (SendMessageToAdmin এর মতোই সামঞ্জস্যপূর্ণ)
            var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.UserId == targetUserId);
            if (session == null)
            {
                session = new ChatSession { UserId = targetUserId };
                _db.ChatSessions.Add(session);
            }
            session.LastMessageAt = DateTime.Now;

            var chatMsg = new ChatMessage
            {
                SenderId = admin.Id,
                ReceiverId = targetUserId,
                Message = message,
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

            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveMessage", payload);
            await Clients.Caller.SendAsync("MessageSent", new { payload, targetUserId });
        }

        // ── Messages পড়া হয়েছে mark করা ─────────────────────────────────────
        public async Task MarkAsRead(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            var caller = await _userManager.GetUserAsync(Context.User!);
            if (caller == null) return;

            // ✅ আগে কোনো চেক ছিল না — যেকোনো authenticated ইউজার যেকোনো session
            // এর unread reset করতে পারতো। এখন শুধু CanView থাকা staff/admin পারবে।
            var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
            if (!perm.CanView) return;

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

        // ── Multi-connection safe helpers ───────────────────────────────────
        private static void AddConnection(string userId, string connectionId)
        {
            lock (_onlineLock)
            {
                if (!_onlineUsers.TryGetValue(userId, out var set))
                {
                    set = new HashSet<string>();
                    _onlineUsers[userId] = set;
                }
                set.Add(connectionId);
            }
        }

        /// <returns>true হলে ইউজারের এখনো অন্য active connection আছে</returns>
        private static bool RemoveConnection(string userId, string connectionId)
        {
            lock (_onlineLock)
            {
                if (!_onlineUsers.TryGetValue(userId, out var set)) return false;
                set.Remove(connectionId);
                if (set.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                    return false;
                }
                return true;
            }
        }
    }
}