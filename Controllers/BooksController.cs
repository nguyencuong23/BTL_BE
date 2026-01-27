using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder)
        {
            var booksQuery = _context.Books.Include(b => b.Category).AsQueryable();

            // Calculate Totals
            ViewData["TotalTitles"] = await booksQuery.CountAsync();
            ViewData["TotalCopies"] = await booksQuery.SumAsync(b => b.Quantity);

            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";
            ViewData["CurrentSort"] = sortOrder;

            booksQuery = sortOrder switch
            {
                "title_desc" => booksQuery.OrderByDescending(b => b.Title).ThenBy(b => b.BookId),
                "Author" => booksQuery.OrderBy(b => b.Author).ThenBy(b => b.BookId),
                "author_desc" => booksQuery.OrderByDescending(b => b.Author).ThenBy(b => b.BookId),
                "Date" => booksQuery.OrderBy(b => b.BookId), // Oldest = Smallest ID
                "date_desc" => booksQuery.OrderByDescending(b => b.BookId), // Newest = Largest ID
                _ => booksQuery.OrderBy(b => b.Title).ThenBy(b => b.BookId),
            };

            return View(await booksQuery.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();
            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookId,Title,Author,Publisher,CategoryId,PublishYear,Quantity,Location")] Book book,
            IFormFile? imageFile)
        {
            if (book.PublishYear > DateTime.Now.Year)
            {
                ModelState.AddModelError("PublishYear", "Năm xuất bản không được lớn hơn năm hiện tại");
            }

            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
                return View(book);
            }

            if (imageFile is not null && imageFile.Length > 0)
            {
                var saveResult = await SaveBookImageAsync(imageFile);
                if (!saveResult.Success)
                {
                    ModelState.AddModelError("", saveResult.Error!);
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
                    return View(book);
                }
                book.ImagePath = saveResult.Path;
            }

            try
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Thêm sách '{book.Title}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                if (errorMsg.Contains("PRIMARY KEY") || errorMsg.Contains("duplicate"))
                {
                    ModelState.AddModelError("BookId", $"Mã sách '{book.BookId}' đã tồn tại. Vui lòng chọn mã khác.");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể thêm sách. Lỗi cơ sở dữ liệu: " + errorMsg);
                }
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
                return View(book);
            }
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            [Bind("BookId,Title,Author,Publisher,CategoryId,PublishYear,Quantity,Location")] Book book,
            IFormFile? imageFile)
        {
            if (id != book.BookId) return NotFound();

            var dbBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
            if (dbBook == null) return NotFound();

            if (book.PublishYear > DateTime.Now.Year)
            {
                ModelState.AddModelError("PublishYear", "Năm xuất bản không được lớn hơn năm hiện tại");
            }

            // Fix: Giữ nguyên thể loại cũ vì form Edit không có field này
            book.CategoryId = dbBook.CategoryId;
            ModelState.Remove("CategoryId");

            if (!ModelState.IsValid)
            {
                // giữ ảnh cũ để view không bị mất dữ liệu
                book.ImagePath = dbBook.ImagePath;
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
                return View(book);
            }

            // mặc định giữ ảnh cũ
            book.ImagePath = dbBook.ImagePath;

            // nếu upload ảnh mới
            if (imageFile is not null && imageFile.Length > 0)
            {
                var saveResult = await SaveBookImageAsync(imageFile);
                if (!saveResult.Success)
                {
                    ModelState.AddModelError("", saveResult.Error!);
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
                    return View(book);
                }

                // xóa ảnh cũ (nếu có)
                DeleteFileIfExists(dbBook.ImagePath);

                book.ImagePath = saveResult.Path;
            }

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật sách '{book.Title}' thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.BookId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // xóa ảnh nếu có
                DeleteFileIfExists(book.ImagePath);

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa sách '{book.Title}' thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(string id)
            => _context.Books.Any(e => e.BookId == id);

        private async Task<(bool Success, string? Path, string? Error)> SaveBookImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowed.Contains(ext))
                return (false, null, "Chỉ cho phép ảnh JPG/JPEG/PNG/WEBP.");

            // (tuỳ chọn) giới hạn 5MB
            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "Ảnh quá lớn (tối đa 5MB).");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            var relativePath = $"/uploads/books/{fileName}";
            return (true, relativePath, null);
        }

        private void DeleteFileIfExists(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var physical = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);
        }
    }
}