using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminOrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

 // Status filter order — user details 
        public async Task<List<Order>> GetFilteredOrdersAsync(string? status)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

 // Pending order count (badge )
        public async Task<int> GetPendingOrdersCountAsync()
        {
            return await _db.Orders.CountAsync(o => o.Status == "Pending");
        }

 // ReturnRequest GET — order + user 
        public async Task<Order?> GetOrderWithUserAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

 // ReturnRequest GET — product 
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _db.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

 // order+product return request 
        public async Task<bool> HasReturnRequestAsync(int orderId, int productId, string userId)
        {
            return await _db.ReturnRequests
                .AnyAsync(r => r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);
        }

 // Order user verify 
        public async Task<Order?> GetUserOrderAsync(int orderId, string userId)
        {
            return await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

 // return request save 
        public async Task AddReturnRequestAsync(int orderId, int productId, string userId, string reason)
        {
            _db.ReturnRequests.Add(new ReturnRequest
            {
                OrderId = orderId,
                ProductId = productId,
                UserId = userId,
                Reason = reason,
                Status = "Pending"
            });
            await _db.SaveChangesAsync();
        }

 // Order detail page — user, orderDetails, product, images 
        public async Task<Order?> GetOrderDetailAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

 // Id order (UpdateStatus Delete )
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders.FindAsync(id);
        }

 // Order status UpdatedAt update 
        public async Task UpdateStatusAsync(Order order, string status)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

 // Order delete 
        public async Task DeleteAsync(Order order)
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
        }

        // ── Delivery & Tracking ──────────────────────────────────────────────────

        public async Task<List<Order>> GetDeliveryOrdersAsync(string? status, string? search, string? assignedEmployeeId = null)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedEmployee)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.TrackingLogs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(assignedEmployeeId))
            {
                query = query.Where(o => o.AssignedEmployeeId == assignedEmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(o => o.Id.ToString() == s
                    || o.Phone.Contains(s)
                    || o.ShippingAddress.Contains(s)
                    || (o.TrackingNumber != null && o.TrackingNumber.Contains(s))
                    || (o.User != null && (o.User.FullName.Contains(s) || (o.User.Email != null && o.User.Email.Contains(s))))
                    || (o.AssignedEmployee != null && o.AssignedEmployee.FullName.Contains(s)));
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<Order?> GetDeliveryOrderAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedEmployee)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p!.Images)
                .Include(o => o.TrackingLogs)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<ApplicationUser>> GetDeliveryStaffListAsync()
        {
            return await _db.Users
                .Where(u => u.UserType == "Employee" || u.UserType == "Admin")
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task AssignRiderAsync(int orderId, string employeeId, DateTime? estimatedDate, string? notes, string updatedBy)
        {
            var order = await _db.Orders.Include(o => o.AssignedEmployee).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            var employee = await _db.Users.FindAsync(employeeId);
            if (employee == null) return;

            order.AssignedEmployeeId = employeeId;
            order.AssignedAt = DateTime.Now;
            if (estimatedDate.HasValue) order.EstimatedDeliveryDate = estimatedDate.Value;
            if (!string.IsNullOrWhiteSpace(notes)) order.DeliveryNotes = notes;
            if (string.IsNullOrWhiteSpace(order.TrackingNumber))
            {
                order.TrackingNumber = $"TRK-{DateTime.Now:yyyyMM}-{order.Id:D5}";
            }

            _db.OrderTrackingLogs.Add(new OrderTrackingLog
            {
                OrderId = orderId,
                Status = "Rider Assigned",
                Title = "Delivery Partner Assigned",
                Description = $"Order assigned to {employee.FullName}" + (!string.IsNullOrEmpty(employee.PhoneNumber) ? $" ({employee.PhoneNumber})" : "") + (!string.IsNullOrWhiteSpace(notes) ? $". Note: {notes}" : ""),
                Location = order.DeliveryZone,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task UpdateDeliveryStatusAsync(int orderId, string status, string? location, string? notes, string updatedBy)
        {
            var order = await _db.Orders.Include(o => o.AssignedEmployee).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            order.Status = status;
            order.UpdatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(order.TrackingNumber))
            {
                order.TrackingNumber = $"TRK-{DateTime.Now:yyyyMM}-{order.Id:D5}";
            }

            string logTitle = status switch
            {
                "Pending" => "Order Placed & Awaiting Confirmation",
                "Processing" => "Order Confirmed & Being Prepared",
                "Shipped" => "Order Dispatched & In Transit",
                "Out for Delivery" => "Out for Delivery",
                "Completed" => "Delivered Successfully",
                "Cancelled" => "Order Cancelled",
                _ => $"Status updated to {status}"
            };

            string logDesc = notes ?? "";

            if (status == "Out for Delivery")
            {
                order.OutForDeliveryAt = DateTime.Now;
                if (string.IsNullOrWhiteSpace(order.DeliveryOtp))
                {
                    order.DeliveryOtp = Random.Shared.Next(100000, 999999).ToString();
                    order.IsOtpVerified = false;
                }

                var riderName = order.AssignedEmployee?.FullName ?? "Delivery Partner";
                var riderPhone = order.AssignedEmployee?.PhoneNumber;
                var riderText = !string.IsNullOrEmpty(riderPhone) ? $"{riderName} ({riderPhone})" : riderName;

                logDesc = $"Your parcel is out for delivery with {riderText}. Customer Delivery OTP: {order.DeliveryOtp}." +
                          (!string.IsNullOrWhiteSpace(notes) ? $" Note: {notes}" : "");
            }
            else if (status == "Completed")
            {
                order.DeliveredAt = DateTime.Now;
                if (string.IsNullOrWhiteSpace(logDesc))
                    logDesc = "Order has been delivered to customer.";
            }

            _db.OrderTrackingLogs.Add(new OrderTrackingLog
            {
                OrderId = orderId,
                Status = status,
                Title = logTitle,
                Description = logDesc,
                Location = location ?? order.DeliveryZone,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> VerifyOtpAndCompleteDeliveryAsync(int orderId, string otp, string? notes, string updatedBy)
        {
            var order = await _db.Orders.Include(o => o.AssignedEmployee).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return (false, "Order not found.");

            if (!string.IsNullOrWhiteSpace(order.DeliveryOtp))
            {
                if (string.IsNullOrWhiteSpace(otp) || order.DeliveryOtp.Trim() != otp.Trim())
                {
                    return (false, "Invalid Delivery OTP. Please double-check with the customer.");
                }
            }

            order.IsOtpVerified = true;
            order.Status = "Completed";
            order.DeliveredAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            _db.OrderTrackingLogs.Add(new OrderTrackingLog
            {
                OrderId = orderId,
                Status = "Completed",
                Title = "Delivered (OTP Verified)",
                Description = "Order successfully delivered to customer after OTP verification." + (!string.IsNullOrWhiteSpace(notes) ? $" Note: {notes}" : ""),
                Location = order.DeliveryZone,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return (true, $"Order #{orderId} successfully delivered and verified via OTP!");
        }

        public async Task AdminOverrideCompleteDeliveryAsync(int orderId, string? notes, string updatedBy)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            order.Status = "Completed";
            order.DeliveredAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            _db.OrderTrackingLogs.Add(new OrderTrackingLog
            {
                OrderId = orderId,
                Status = "Completed",
                Title = "Delivered (Direct / Admin Override)",
                Description = "Order marked as delivered." + (!string.IsNullOrWhiteSpace(notes) ? $" Reason/Note: {notes}" : ""),
                Location = order.DeliveryZone,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task AddTrackingLogAsync(int orderId, string status, string title, string? description, string? location, string? updatedBy)
        {
            _db.OrderTrackingLogs.Add(new OrderTrackingLog
            {
                OrderId = orderId,
                Status = status,
                Title = title,
                Description = description,
                Location = location,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }
    }
}

