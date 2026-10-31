using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class AdminChatRepository : IAdminChatRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminChatRepository(ApplicationDbContext db)
        {
            _db = db;
        }

 // session — user , message 
        public async Task<List<ChatSession>> GetAllSessionsAsync()
        {
            return await _db.ChatSessions
                .Include(s => s.User)
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();
        }

 // session unread count (navbar badge )
        public async Task<int> GetTotalUnreadCountAsync()
        {
            return await _db.ChatSessions.SumAsync(s => s.UnreadCount);
        }

 // user messages — 
        public async Task<List<ChatMessage>> GetConversationMessagesAsync(string userId)
        {
            return await _db.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .Take(200)
                .ToListAsync();
        }

 // UserId session 
        public async Task<ChatSession?> GetSessionByUserIdAsync(string userId)
        {
            return await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

 // Session UnreadCount save 
        public async Task ResetSessionUnreadCountAsync(ChatSession session)
        {
            session.UnreadCount = 0;
            await _db.SaveChangesAsync();
        }

 // User unread messages IsRead = true 
        public async Task MarkUserMessagesAsReadAsync(string userId)
        {
            await _db.ChatMessages
                .Where(m => m.SenderId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

            await _db.SaveChangesAsync();
        }
    }
}

