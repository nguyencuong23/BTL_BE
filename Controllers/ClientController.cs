using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Models.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;

namespace QuanLyThuVienTruongHoc.Controllers
{
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;
        private readonly ApplicationDbContext _context;

        public ClientController(ILogger<ClientController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch 10 latest books by ID desc
            var latestBooks = await _context.Books
                .AsNoTracking()
                .OrderByDescending(b => b.BookId)
                .Take(10)
                .ToListAsync();

            return View(latestBooks);
        }

        public async Task<IActionResult> Search(string search, string sortOrder)
        {
            var books = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                books = books.Where(b => b.Title.ToLower().Contains(search) || 
                                         b.Author.ToLower().Contains(search));
            }

            return View(await books.ToListAsync());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(string bookId, DateTime dueDate)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["Error"] = "Sách không tồn tại.";
                return RedirectToAction(nameof(Search));
            }

            if (book.Quantity <= 0)
            {
                TempData["Error"] = "Sách đã hết, vui lòng chọn cuốn khác.";
                return RedirectToAction(nameof(Search));
            }

            var loan = new QuanLyThuVienTruongHoc.Models.Library.Loan
            {
                BookId = bookId,
                UserId = user.Id,
                BorrowDate = DateTime.Now,
                DueDate = dueDate,
                Status = Models.Library.LoanStatus.DangMuon, // DangMuon (1)
                Fine = 0
            };

            _context.Loans.Add(loan);
            book.Quantity -= 1;
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Đăng ký mượn sách thành công!";
            return RedirectToAction(nameof(Search));
        }

        public IActionResult News()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Loans()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var myLoans = await _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.BorrowDate)
                .ToListAsync();

            return View(myLoans);
        }

        [Authorize]
        public IActionResult UserOnly()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetBooksJson()
        {
            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Select(b => new {
                    id = b.BookId,
                    title = b.Title,
                    author = b.Author,
                    image = b.ImagePath,
                    description = "", // Or mapped field
                    available = b.Quantity,
                    publisher = b.Publisher,
                    year = b.PublishYear,
                    location = b.Location,
                    category = b.Category.Name
                })
                .ToListAsync();

            return Json(books);
        }
    }
}