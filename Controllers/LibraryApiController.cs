using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Library;
using QuanLyThuVienTruongHoc.Models.Users;
using QuanLyThuVienTruongHoc.Models.ViewModels;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Route("api/library")]
    [ApiController]
    public class LibraryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibraryApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("books")]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Shelf)
                .Select(b => new {
                    id = b.BookId,
                    title = b.Title,
                    author = b.Author,
                    available = b.Quantity,
                    image = b.ImagePath ?? "",
                    location = b.Shelf != null
                               ? b.Shelf.Name
                               : (b.Location ?? "Chưa xếp kệ")
                })
                .ToListAsync();

            return Ok(books);
        }
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestViewModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dữ liệu gửi lên không hợp lệ.");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. KIỂM TRA SÁCH
                var book = await _context.Books.FindAsync(request.BookId);
                if (book == null) return NotFound("Sách không tồn tại.");

                if (book.Quantity <= 0)
                    return BadRequest("Sách này đã hết, vui lòng chọn cuốn khác.");

                // B. KIỂM TRA SINH VIÊN
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.StudentCode == request.StudentId || u.Username == request.StudentId);

                if (user == null)
                {
                    return BadRequest($"Mã sinh viên '{request.StudentId}' chưa tồn tại trong hệ thống. Vui lòng liên hệ thư viện để tạo thẻ.");
                }
                var loan = new Loan
                {
                    BookId = book.BookId,
                    UserId = user.Id,
                    BorrowDate = DateTime.Now,
                    DueDate = request.ReturnDate,
                    Status = 0,
                    Fine = 0
                };

                _context.Loans.Add(loan);
                book.Quantity -= 1;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đăng ký mượn thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}