using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;

namespace ShopManagementSystem.Repository.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _db;

        public ChatRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // Session থাকলে return করে, না থাকলে নতুন তৈরি করে
        public async Task<ChatSession> GetOrCreateSessionAsync(string userId)
        {
            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (session == null)
            {
                session = new ChatSession { UserId = userId };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            return session;
        }

        // User এর সব messages — পুরনো থেকে নতুন, সর্বোচ্চ ১০০টি
        public async Task<List<ChatMessage>> GetMessagesAsync(string userId)
        {
            return await _db.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .Take(100)
                .ToListAsync();
        }

        // User এর সব unread messages IsRead = true করে
        public async Task MarkMessagesAsReadAsync(string userId)
        {
            await _db.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        // Session এর UnreadCount শূন্য করে save করে
        public async Task ResetUnreadCountAsync(ChatSession session)
        {
            session.UnreadCount = 0;
            await _db.SaveChangesAsync();
        }

        // Admin থেকে আসা unread message count (AJAX poll এর জন্য)
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead && m.IsFromAdmin);
        }
    }
}
