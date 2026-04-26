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

        // সব session — user সহ, সর্বশেষ message অনুযায়ী সাজানো
        public async Task<List<ChatSession>> GetAllSessionsAsync()
        {
            return await _db.ChatSessions
                .Include(s => s.User)
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();
        }

        // সব session এর মোট unread count (navbar badge এর জন্য)
        public async Task<int> GetTotalUnreadCountAsync()
        {
            return await _db.ChatSessions.SumAsync(s => s.UnreadCount);
        }

        // নির্দিষ্ট user এর সব messages — সর্বোচ্চ ২০০টি
        public async Task<List<ChatMessage>> GetConversationMessagesAsync(string userId)
        {
            return await _db.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .Take(200)
                .ToListAsync();
        }

        // UserId দিয়ে session খোঁজে
        public async Task<ChatSession?> GetSessionByUserIdAsync(string userId)
        {
            return await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Session এর UnreadCount শূন্য করে save করে
        public async Task ResetSessionUnreadCountAsync(ChatSession session)
        {
            session.UnreadCount = 0;
            await _db.SaveChangesAsync();
        }

        // User এর unread messages গুলো IsRead = true করে
        public async Task MarkUserMessagesAsReadAsync(string userId)
        {
            await _db.ChatMessages
                .Where(m => m.SenderId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

            await _db.SaveChangesAsync();
        }
    }
}
