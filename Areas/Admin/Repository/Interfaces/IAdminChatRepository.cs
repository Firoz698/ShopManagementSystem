using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface IAdminChatRepository
    {
        Task<List<ChatSession>> GetAllSessionsAsync();
        Task<int> GetTotalUnreadCountAsync();
        Task<List<ChatMessage>> GetConversationMessagesAsync(string userId);
        Task<ChatSession?> GetSessionByUserIdAsync(string userId);
        Task ResetSessionUnreadCountAsync(ChatSession session);
        Task MarkUserMessagesAsReadAsync(string userId);
    }
}
