using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;

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
            // 1. Books
            var totalBooks = await _context.Books.SumAsync(b => b.Quantity); // Tổng số lượng bản sách
            var totalTitles = await _context.Books.CountAsync(); // Tổng số đầu sách (unique titles)

            // 2. Readers
            var totalStudents = await _context.Users.CountAsync(u => u.Role == 2);
            var activeBorrowers = await _context.Loans
                .Where(l => l.ReturnDate == null)
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            // 3. Borrowing
            var today = DateTime.Today;
            var loans = _context.Loans.AsQueryable();
            var totalBorrowing = await loans.CountAsync(l => l.ReturnDate == null);
            var borrowedToday = await loans.CountAsync(l => l.BorrowDate.Date == today);
            
            var threeDaysLater = DateTime.Now.AddDays(3);
            var dueSoonCount = await loans.CountAsync(l => 
                l.ReturnDate == null && 
                l.DueDate <= threeDaysLater && 
                l.DueDate >= DateTime.Now);

            // 4. Overdue
            var totalOverdue = await loans.CountAsync(l => l.ReturnDate == null && l.DueDate < DateTime.Now);
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var overdueMoreThan7Days = await loans.CountAsync(l => 
                l.ReturnDate == null && 
                l.DueDate < sevenDaysAgo);
            
            var totalFine = await _context.Users.SumAsync(u => u.TotalFine);

            return Ok(new
            {
                // Card 1
                totalBooks,
                totalTitles,
                
                // Card 2
                totalStudents,
                activeBorrowers,
                
                // Card 3
                totalBorrowing,
                borrowedToday,
                dueSoonCount,
                
                // Card 4
                totalOverdue,
                overdueMoreThan7Days,
                totalFine
            });
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalBooks = await _context.Books.SumAsync(b => b.Quantity);
            var totalReaders = await _context.Users.CountAsync(u => u.Role == 2); // Role 2 is User/Student
            
            // Loan status
            var loans = _context.Loans.AsQueryable();
            var totalBorrowing = await loans.CountAsync(l => l.ReturnDate == null);
            var totalOverdue = await loans.CountAsync(l => l.ReturnDate == null && l.DueDate < DateTime.Now);
            var totalReturned = await loans.CountAsync(l => l.ReturnDate != null);
            var totalFine = await _context.Users.SumAsync(u => u.TotalFine);

            // Recent loans
            var recentLoans = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .OrderByDescending(l => l.BorrowDate)
                .Take(5)
                .Select(l => new
                {
                    l.LoanId,
                    UserFullName = l.User.FullName,
                    BookTitle = l.Book.Title,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    Status = l.ReturnDate != null ? "returned" : (l.DueDate < DateTime.Now ? "overdue" : "borrowing")
                })
                .ToListAsync();

            // Top borrowed books
            var topBooks = await _context.Books
                .OrderByDescending(b => b.Loans.Count)
                .Take(5)
                .Select(b => new
                {
                    b.Title,
                    b.Author,
                    BorrowCount = b.Loans.Count
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
                totalReaders,
                totalBorrowing,
                totalOverdue,
                totalReturned,
                totalFine,
                recentLoans,
                topBooks,
                categoryStats
            });
        }

        [HttpGet("monthly-stats")]
        public async Task<IActionResult> GetMonthlyStats()
        {
            var currentYear = DateTime.Now.Year;
            
            // Group loans by month in current year
            var monthlyData = await _context.Loans
                .Where(l => l.BorrowDate.Year == currentYear)
                .GroupBy(l => l.BorrowDate.Month)
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

            // 1. Borrow Volume (Loans created per day)
            var borrowData = await _context.Loans
                .Where(l => l.BorrowDate >= startDate && l.BorrowDate <= endDate.AddDays(1)) // Include full end date
                .GroupBy(l => l.BorrowDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var borrowVolume = Enumerable.Range(0, 30)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = borrowData.FirstOrDefault(d => d.Date == date)?.Count ?? 0
                })
                .ToList();

            // 2. Active Loans History (Loans active at end of each day)
            // Get all loans that could have been active during this period:
            // Borrowed <= EndDate AND (Returned is NULL OR Returned > StartDate)
            var relevantLoans = await _context.Loans
                .Where(l => l.BorrowDate.Date <= endDate && (l.ReturnDate == null || l.ReturnDate.Value.Date > startDate))
                .Select(l => new { l.BorrowDate, l.ReturnDate })
                .ToListAsync();

            var activeLoans = Enumerable.Range(0, 30)
                .Select(offset => startDate.AddDays(offset))
                .Select(date =>
                {
                    // Active = Borrowed <= CurrentLoopDate AND (NotReturned OR Returned > CurrentLoopDate)
                    var count = relevantLoans.Count(l => 
                        l.BorrowDate.Date <= date && 
                        (l.ReturnDate == null || l.ReturnDate.Value.Date > date));
                    return new { Date = date.ToString("yyyy-MM-dd"), Count = count };
                })
                .ToList();

            return Ok(new { borrowVolume, activeLoans });
        }
    }
}
