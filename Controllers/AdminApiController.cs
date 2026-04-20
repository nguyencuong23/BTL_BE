using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Commerce;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            // Books
            var totalBooks = await _context.Books.SumAsync(b => b.Quantity);
            var totalTitles = await _context.Books.CountAsync();

            // Customers
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == 2);

            // Orders
            var orders = _context.Orders.AsQueryable();
            var totalOrders = await orders.CountAsync();
            var pendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var pendingBankTransfers = await orders.CountAsync(o =>
                o.PaymentMethod == PaymentMethod.BankTransfer &&
                o.PaymentStatus == PaymentStatus.PendingConfirmation);

            var today = DateTime.Today;
            var ordersToday = await orders.CountAsync(o => o.CreatedAt.Date == today);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var revenueThisMonth = await orders
                .Where(o => o.CreatedAt.Year == currentYear && o.CreatedAt.Month == currentMonth && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            return Ok(new
            {
                totalBooks,
                totalTitles,
                totalCustomers,
                totalOrders,
                pendingOrders,
                pendingBankTransfers,
                ordersToday,
                revenueThisMonth
            });
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalBooks = await _context.Books.SumAsync(b => b.Quantity);
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == 2);

            var orders = _context.Orders.AsQueryable();
            var totalPending = await orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var totalShipping = await orders.CountAsync(o => o.Status == OrderStatus.Shipping);
            var totalDelivered = await orders.CountAsync(o => o.Status == OrderStatus.Delivered);
            var totalCancelled = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    UserFullName = o.User.FullName,
                    o.Total,
                    o.CreatedAt,
                    Status = o.Status.ToString(),
                    PaymentMethod = o.PaymentMethod.ToString(),
                    PaymentStatus = o.PaymentStatus.ToString()
                })
                .ToListAsync();

            // Top selling books (by delivered qty)
            var topBooks = await _context.OrderItems
                .Where(i => i.Order != null && i.Order.Status != OrderStatus.Cancelled)
                .GroupBy(i => i.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .Join(_context.Books, x => x.BookId, b => b.BookId, (x, b) => new
                {
                    b.Title,
                    b.Author,
                    SoldQuantity = x.Quantity,
                    Revenue = x.Revenue
                })
                .ToListAsync();

            // Categories data for pie chart
            var categoryStats = await _context.Categories
                .Select(c => new
                {
                    Label = c.Name,
                    Count = c.Books.Count()
                })
                .ToListAsync();

            return Ok(new
            {
                totalBooks,
                totalCustomers,
                totalPending,
                totalShipping,
                totalDelivered,
                totalCancelled,
                recentOrders,
                topBooks,
                categoryStats
            });
        }

        [HttpGet("monthly-stats")]
        public async Task<IActionResult> GetMonthlyStats()
        {
            var currentYear = DateTime.Now.Year;
            
            var monthlyData = await _context.Orders
                .Where(o => o.CreatedAt.Year == currentYear && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Fill missing months with 0
            var result = Enumerable.Range(1, 12).Select(month => new
            {
                Month = month,
                Count = monthlyData.FirstOrDefault(m => m.Month == month)?.Count ?? 0
            }).ToList();

            return Ok(result);
        }
        [HttpGet("borrowing-trends")]
        public async Task<IActionResult> GetBorrowingTrends()
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-29); // 30 days range

            var orderData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate.AddDays(1) && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var orderVolume = Enumerable.Range(0, 30)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = orderData.FirstOrDefault(d => d.Date == date)?.Count ?? 0
                })
                .ToList();

            var revenueData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate.AddDays(1) && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Total) })
                .ToListAsync();

            var revenue = Enumerable.Range(0, 30)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Revenue = revenueData.FirstOrDefault(d => d.Date == date)?.Revenue ?? 0
                })
                .ToList();

            return Ok(new { orderVolume, revenue });
        }
    }
}
