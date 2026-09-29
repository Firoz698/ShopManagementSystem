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

        // userId -> all active connectionIds for that user (multi-tab/device safe)
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

            // Check actual "Chat" permission - if user has permission, join Admins group
            var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
            if (perm.CanView)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                await Clients.Caller.SendAsync("AdminConnected");
            }
            else
            {
                // Regular customer
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

                // Only send "Offline" when user has no remaining active connections
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

        // ── User -> Admin Send Message ────────────────────────────────────────
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

        // ── Admin -> User Send Message ────────────────────────────────────────
        public async Task SendMessageToUser(string targetUserId, string message)
        {
            var admin = await _userManager.GetUserAsync(Context.User!);
            if (admin == null || string.IsNullOrWhiteSpace(targetUserId)) return;

            message = (message ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(message) || message.Length > MaxMessageLength) return;

            // Check actual permission - reject if CanCreate is false
            var perm = await _permissionChecker.GetPermissionsAsync(Context.User!, "Chat");
            if (!perm.CanCreate)
            {
                await Clients.Caller.SendAsync("Error", "You do not have permission to send messages.");
                return;
            }

            // Create session if it doesn't exist
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

        // ── Mark Messages as Read ─────────────────────────────────────────────
        public async Task MarkAsRead(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            var caller = await _userManager.GetUserAsync(Context.User!);
            if (caller == null) return;

            // Only staff/admin with CanView permission can mark as read
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

        // ── Check if user is online ──────────────────────────────────────────
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

        /// <returns>true if user still has other active connections</returns>
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