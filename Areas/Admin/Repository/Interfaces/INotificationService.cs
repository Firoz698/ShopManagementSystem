namespace ShopManagementSystem.Interfaces
{
    public interface INotificationService
    {
        Task SendNewOrderNotificationAsync(int orderId, string customerName, decimal totalAmount);
        Task SendNewChatMessageNotificationAsync(string customerName, string messageText);
    }
}