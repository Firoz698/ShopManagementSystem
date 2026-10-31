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

 // Session return , 
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

 // User messages — , 
        public async Task<List<ChatMessage>> GetMessagesAsync(string userId)
        {
            return await _db.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .Take(100)
                .ToListAsync();
        }

 // User unread messages IsRead = true 
        public async Task MarkMessagesAsReadAsync(string userId)
        {
            await _db.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

 // Session UnreadCount save 
        public async Task ResetUnreadCountAsync(ChatSession session)
        {
            session.UnreadCount = 0;
            await _db.SaveChangesAsync();
        }

 // Admin unread message count (AJAX poll )
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead && m.IsFromAdmin);
        }

 // ── : User → Admin message + admin session unread ──
        public async Task<ChatMessage> AddUserMessageAsync(string userId, string messageText)
        {
            var message = new ChatMessage
            {
                SenderId = userId,
                ReceiverId = null,          // Admin user , null
                Message = messageText,
                IsFromAdmin = false,
                IsRead = false,
                SentAt = DateTime.Now
            };

            _db.ChatMessages.Add(message);

            var session = await GetOrCreateSessionAsync(userId);
            session.UnreadCount += 1;      // Admin panel unread
            session.LastMessageAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return message;
        }
    }
}
