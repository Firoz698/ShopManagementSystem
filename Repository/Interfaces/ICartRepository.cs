using ShopManagementSystem.Models;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Repository.Interfaces
{
    public interface ICartRepository
    {
        // Cart
        Task<List<Cart>> GetCartItemsAsync(string userId);
        Task<Cart?> GetCartItemAsync(string userId, int productId, int? productSizeId);
        Task<Cart?> GetCartByIdAsync(int cartId, string userId);
        Task AddToCartAsync(string userId, int productId, int? productSizeId, int quantity);
        Task UpdateCartQuantityAsync(Cart cart, int quantity);
        Task RemoveFromCartAsync(Cart cart);
        Task ClearCartAsync(List<Cart> items);
        Task<int> GetCartCountAsync(string userId);

        // Product (cart এর জন্য)
        Task<Product?> GetActiveProductWithSizesAsync(int productId);

        // Order
        Task<Order> CreateOrderAsync(string userId, CheckoutViewModel vm, decimal total);
        Task AddOrderDetailsAsync(int orderId, List<Cart> items);
        Task DeductStockAsync(List<Cart> items);
        Task<Order?> GetOrderConfirmationAsync(int orderId, string userId);
        Task<List<Order>> GetMyOrdersAsync(string userId);

        // Payment
        Task<PaymentTransaction> CreatePaymentTransactionAsync(int orderId, string tranId, decimal amount);
        Task<PaymentTransaction?> GetPaymentByTranIdAsync(string tranId);
        Task UpdatePaymentStatusAsync(PaymentTransaction payment, string status, string? valId = null, string? cardType = null);
        Task UpdateOrderStatusAsync(Order? order, string status);
    }
}
