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

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult TraCuu()
        {
            return View();
        }

        public IActionResult News()
        {
            return View();
        }

        [Authorize]
        public IActionResult Payback()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Paypack([FromServices] ApplicationDbContext context, string? bookId)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                return RedirectToAction("TraCuu");
            }

            var book = await context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Shelf)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                return NotFound();
            }

            return View("Payback", book);
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
    }
}