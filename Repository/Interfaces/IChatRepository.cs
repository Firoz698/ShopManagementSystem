using ShopManagementSystem.Models;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatSession> GetOrCreateSessionAsync(string userId);
        Task<List<ChatMessage>> GetMessagesAsync(string userId);
        Task MarkMessagesAsReadAsync(string userId);
        Task ResetUnreadCountAsync(ChatSession session);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
