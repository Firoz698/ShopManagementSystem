using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;

        public CartRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Cart ─────────────────────────────────────────────────────────────────

        public async Task<List<Cart>> GetCartItemsAsync(string userId)
        {
            return await _db.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product).ThenInclude(p => p!.Images)
                .Include(c => c.ProductSize)
                .ToListAsync();
        }

        public async Task<Cart?> GetCartItemAsync(string userId, int productId, int? productSizeId)
        {
            return await _db.Carts.FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.ProductId == productId &&
                c.ProductSizeId == productSizeId);
        }

        public async Task<Cart?> GetCartByIdAsync(int cartId, string userId)
        {
            return await _db.Carts
                .Include(c => c.Product)
                .Include(c => c.ProductSize)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
        }

        public async Task AddToCartAsync(string userId, int productId, int? productSizeId, int quantity)
        {
            var cart = await GetCartItemAsync(userId, productId, productSizeId);

            if (cart == null)
            {
                _db.Carts.Add(new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductSizeId = productSizeId,
                    Quantity = quantity
                });
            }
            else
            {
                var product = await GetActiveProductWithSizesAsync(productId);
                int maxStock = productSizeId.HasValue
                    ? product?.Sizes.FirstOrDefault(s => s.Id == productSizeId.Value)?.Stock ?? 0
                    : product?.Stock ?? 0;

                cart.Quantity = Math.Min(cart.Quantity + quantity, maxStock);
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateCartQuantityAsync(Cart cart, int quantity)
        {
            int maxStock = cart.ProductSize?.Stock ?? cart.Product!.Stock;
            cart.Quantity = Math.Max(1, Math.Min(quantity, maxStock));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(Cart cart)
        {
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
        }

        public async Task ClearCartAsync(List<Cart> items)
        {
            _db.Carts.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _db.Carts.CountAsync(c => c.UserId == userId);
        }

        // ── Product ──────────────────────────────────────────────────────────────

        public async Task<Product?> GetActiveProductWithSizesAsync(int productId)
        {
            return await _db.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        // ── Order ────────────────────────────────────────────────────────────────

        // status প্যারামিটার যোগ হয়েছে — Online payment এর জন্য "Pending Payment" দিয়ে শুরু হবে
        public async Task<Order> CreateOrderAsync(string userId, CheckoutViewModel vm, decimal total, string status = "Pending")
        {
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = vm.ShippingAddress,
                Phone = vm.Phone,
                PaymentMethod = vm.PaymentMethod,
                DeliveryZone = vm.DeliveryZone,
                Notes = vm.Notes,
                TotalAmount = total,
                Status = status,

                PaymentMethodSettingId = vm.PaymentMethodSettingId,
                SenderNumber = vm.SenderNumber,
                PaymentTransactionId = vm.TransactionId
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task AddOrderDetailsAsync(int orderId, List<Cart> items)
        {
            foreach (var item in items)
            {
                var price = item.ProductSize?.SalesPrice ?? item.Product!.DiscountPrice ?? item.Product!.Price;
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = price
                });
            }
            await _db.SaveChangesAsync();
        }

        public async Task DeductStockAsync(List<Cart> items)
        {
            foreach (var item in items)
            {
                if (item.ProductSize != null)
                    item.ProductSize.Stock -= item.Quantity;
                else
                    item.Product!.Stock -= item.Quantity;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderConfirmationAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        // ── নতুন: PaymentSuccess/IPN এ order lookup করার জন্য (UserId ভ্যালিডেশন ছাড়াই, কারণ gateway callback এ User লগইন নাও থাকতে পারে) ──
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetMyOrdersAsync(string userId)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(Order? order, string status)
        {
            if (order == null) return;
            order.Status = status;
            await _db.SaveChangesAsync();
        }

        // ── Payment ──────────────────────────────────────────────────────────────

        public async Task<PaymentTransaction> CreatePaymentTransactionAsync(int orderId, string tranId, decimal amount)
        {
            var txn = new PaymentTransaction
            {
                OrderId = orderId,
                TransactionId = tranId,
                Amount = amount,
                Status = "Pending"
            };
            _db.PaymentTransactions.Add(txn);
            await _db.SaveChangesAsync();
            return txn;
        }

        public async Task<PaymentTransaction?> GetPaymentByTranIdAsync(string tranId)
        {
            return await _db.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == tranId);
        }

        public async Task UpdatePaymentStatusAsync(PaymentTransaction payment, string status, string? valId = null, string? cardType = null)
        {
            payment.Status = status;
            payment.UpdatedAt = DateTime.Now;
            if (valId != null) payment.ValidationId = valId;
            if (cardType != null) payment.PaymentMethod = cardType;
            await _db.SaveChangesAsync();
        }
    }
}