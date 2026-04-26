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

        // User এর সব cart item — product ও image সহ
        public async Task<List<Cart>> GetCartItemsAsync(string userId)
        {
            return await _db.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product).ThenInclude(p => p!.Images)
                .Include(c => c.ProductSize)
                .ToListAsync();
        }

        // নির্দিষ্ট product+size এর cart item খোঁজে
        public async Task<Cart?> GetCartItemAsync(string userId, int productId, int? productSizeId)
        {
            return await _db.Carts.FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.ProductId == productId &&
                c.ProductSizeId == productSizeId);
        }

        // Cart Id দিয়ে cart item খোঁজে
        public async Task<Cart?> GetCartByIdAsync(int cartId, string userId)
        {
            return await _db.Carts
                .Include(c => c.Product)
                .Include(c => c.ProductSize)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
        }

        // Cart এ নতুন item যোগ বা quantity বাড়ায়
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

        // Cart item এর quantity update করে (1 থেকে max stock এর মধ্যে)
        public async Task UpdateCartQuantityAsync(Cart cart, int quantity)
        {
            int maxStock = cart.ProductSize?.Stock ?? cart.Product!.Stock;
            cart.Quantity = Math.Max(1, Math.Min(quantity, maxStock));
            await _db.SaveChangesAsync();
        }

        // একটি cart item remove করে
        public async Task RemoveFromCartAsync(Cart cart)
        {
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
        }

        // Order complete হলে সব cart item মুছে ফেলে
        public async Task ClearCartAsync(List<Cart> items)
        {
            _db.Carts.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        // Navbar badge এর জন্য cart item count
        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _db.Carts.CountAsync(c => c.UserId == userId);
        }

        // ── Product ──────────────────────────────────────────────────────────────

        // Active product + sizes (cart add validate এর জন্য)
        public async Task<Product?> GetActiveProductWithSizesAsync(int productId)
        {
            return await _db.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        // ── Order ────────────────────────────────────────────────────────────────

        // নতুন order তৈরি করে save করে
        public async Task<Order> CreateOrderAsync(string userId, CheckoutViewModel vm, decimal total)
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
                Status = "Pending"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        // Cart item গুলো থেকে OrderDetails তৈরি করে
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

        // Order দেওয়ার পর product/size stock কমায়
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

        // Order confirmation page এর জন্য order + details
        public async Task<Order?> GetOrderConfirmationAsync(int orderId, string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        // User এর সব order — নতুন থেকে পুরনো
        public async Task<List<Order>> GetMyOrdersAsync(string userId)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // ── Payment ──────────────────────────────────────────────────────────────

        // নতুন payment transaction তৈরি করে
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

        // TransactionId দিয়ে payment খোঁজে (order সহ)
        public async Task<PaymentTransaction?> GetPaymentByTranIdAsync(string tranId)
        {
            return await _db.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == tranId);
        }

        // Payment status, validation id ও card type update করে
        public async Task UpdatePaymentStatusAsync(PaymentTransaction payment, string status, string? valId = null, string? cardType = null)
        {
            payment.Status = status;
            payment.UpdatedAt = DateTime.Now;
            if (valId != null) payment.ValidationId = valId;
            if (cardType != null) payment.PaymentMethod = cardType;
            await _db.SaveChangesAsync();
        }

        // Order এর status update করে
        public async Task UpdateOrderStatusAsync(Order? order, string status)
        {
            if (order == null) return;
            order.Status = status;
            await _db.SaveChangesAsync();
        }
    }
}