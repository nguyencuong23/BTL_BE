using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models;
using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Quét và tạo thông báo: (1) Còn 7 ngày đến hạn, (2) Quá hạn, (3) Tiền phạt gần ≥30k (sắp bị khóa 50k).
        /// Tránh tạo trùng theo cùng ngày + LoanId/Type.
        /// </summary>
        public async Task CheckAndGenerateNotificationsAsync(int userId)
        {
            var today = DateTime.Today;
            var todayEnd = today.AddDays(1);

            // Phiên mượn đang mượn (chưa trả)
            var activeLoans = await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.UserId == userId && l.ReturnDate == null && l.Status == LoanStatus.DangMuon)
                .ToListAsync();

            var existingToday = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt >= today && n.CreatedAt < todayEnd)
                .Select(n => new { n.RelatedEntityId, n.Type })
                .ToListAsync();

            var existingSet = existingToday
                .Where(x => x.RelatedEntityId.HasValue)
                .Select(x => (x.RelatedEntityId!.Value, x.Type))
                .ToHashSet();

            // Có thông báo FineWarning hôm nay chưa (RelatedEntityId null cho loại này)
            var hasFineWarningToday = existingToday.Any(x => x.Type == NotificationType.FineWarning);

            foreach (var loan in activeLoans)
            {
                var dueDate = loan.DueDate.Date;
                var bookTitle = loan.Book?.Title ?? loan.BookId;

                // 1) Quá hạn: DueDate < Today
                if (dueDate < today)
                {
                    if (!existingSet.Contains((loan.LoanId, NotificationType.Overdue)))
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = userId,
                            Type = NotificationType.Overdue,
                            Title = "Sách trả quá hạn",
                            Message = $"Sách \"{bookTitle}\" đã quá hạn trả (hạn: {dueDate:dd/MM/yyyy}). Vui lòng trả sách.",
                            Link = "/Client/Loans",
                            RelatedEntityId = loan.LoanId,
                            IsRead = false
                        });
                        existingSet.Add((loan.LoanId, NotificationType.Overdue));
                    }
                }
                // 2) Sắp đến hạn: còn 7 ngày trở lại (0 <= (DueDate - Today) <= 7)
                else if (dueDate >= today && (dueDate - today).TotalDays <= 7)
                {
                    if (!existingSet.Contains((loan.LoanId, NotificationType.NearDue)))
                    {
                        var daysLeft = (int)(dueDate - today).TotalDays;
                        _context.Notifications.Add(new Notification
                        {
                            UserId = userId,
                            Type = NotificationType.NearDue,
                            Title = "Sắp đến hạn trả sách",
                            Message = daysLeft == 0
                                ? $"Sách \"{bookTitle}\" hết hạn trả hôm nay ({dueDate:dd/MM/yyyy}). Vui lòng trả đúng hạn."
                                : $"Sách \"{bookTitle}\" còn {daysLeft} ngày đến hạn trả ({dueDate:dd/MM/yyyy}).",
                            Link = "/Client/Loans",
                            RelatedEntityId = loan.LoanId,
                            IsRead = false
                        });
                        existingSet.Add((loan.LoanId, NotificationType.NearDue));
                    }
                }
            }

            // 3) Cảnh báo tiền phạt: tổng nợ (TotalFine - PaidAmount) > 0
            var user = await _context.Users.FindAsync(userId);
            if (user != null && !hasFineWarningToday)
            {
                var debt = user.TotalFine - user.PaidAmount;
                if (debt > 0)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Type = NotificationType.FineWarning,
                        Title = "Cảnh báo tiền phạt",
                        Message = $"Bạn đang có tiền phạt {debt:N0} đ. Vui lòng thanh toán để tránh bị khóa tài khoản.",
                        Link = "/Profile",
                        RelatedEntityId = null,
                        IsRead = false
                    });
                }
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
