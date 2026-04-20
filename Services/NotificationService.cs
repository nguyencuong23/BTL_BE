using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models;
using QuanLyThuVienTruongHoc.Models.Commerce;

namespace QuanLyThuVienTruongHoc.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CheckAndGenerateNotificationsAsync(int userId)
        {
            var today = DateTime.Today;
            var todayEnd = today.AddDays(1);

            var existingToday = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt >= today && n.CreatedAt < todayEnd)
                .Select(n => new { n.RelatedEntityId, n.Type })
                .ToListAsync();

            var existingSet = existingToday
                .Where(x => x.RelatedEntityId.HasValue)
                .Select(x => (x.RelatedEntityId!.Value, x.Type))
                .ToHashSet();

            // E-commerce: pending bank transfer confirmation
            var pendingTransfers = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId &&
                            o.PaymentMethod == PaymentMethod.BankTransfer &&
                            o.PaymentStatus == PaymentStatus.PendingConfirmation &&
                            o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new { o.OrderId, o.OrderCode })
                .ToListAsync();

            foreach (var o in pendingTransfers)
            {
                if (existingSet.Contains((o.OrderId, NotificationType.PaymentPending))) continue;
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Type = NotificationType.PaymentPending,
                    Title = "Chờ xác nhận chuyển khoản",
                    Message = $"Đơn {o.OrderCode} đang chờ admin xác nhận chuyển khoản.",
                    Link = $"/Orders/Details/{o.OrderId}",
                    RelatedEntityId = o.OrderId,
                    IsRead = false
                });
                existingSet.Add((o.OrderId, NotificationType.PaymentPending));
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách thông báo của user, sắp xếp mới nhất trước.
        /// </summary>
        public async Task<List<Notification>> GetNotificationsAsync(int userId, int limit = 50)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Đánh dấu một thông báo đã đọc (chỉ khi thuộc user).
        /// </summary>
        public async Task<bool> MarkReadAsync(int notificationId, int userId)
        {
            var n = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n == null) return false;
            n.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của user là đã đọc.
        /// </summary>
        public async Task MarkAllReadAsync(int userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true));
        }

        /// <summary>
        /// Số thông báo chưa đọc.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
