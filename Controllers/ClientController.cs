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
        public IActionResult Borrow()
        {
            // Legacy route from library version - redirect to shop cart
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult News()
        {
            return View();
        }

        [Authorize]
        public IActionResult Loans()
        {
            // Legacy page - redirect to new Orders page
            return RedirectToAction("Index", "Orders");
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