using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data; // Thay bằng namespace đúng của bạn
using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Route("api/library")] // Đường dẫn API sẽ là /api/library/books
    [ApiController]
    public class LibraryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibraryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy danh sách sách (Dành cho trang chủ sinh viên)
        [HttpGet("books")]
        public async Task<IActionResult> GetBooks()
        {
            // Lấy sách từ Database giống như Admin, nhưng chỉ chọn trường cần thiết
            var books = await _context.Books
                .Select(b => new {
                    id = b.BookId,          // Map sang 'id' cho JS
                    title = b.Title,        // Map sang 'title'
                    author = b.Author,      // Map sang 'author'
                    available = b.Quantity, // Map 'Quantity' sang 'available'
                    image = string.IsNullOrEmpty(b.ImagePath) ? "" : b.ImagePath
                })
                .ToListAsync();

            return Ok(books);
        }

        // 2. API Mượn sách (Xử lý khi sinh viên bấm nút)
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequest request)
        {
            var book = await _context.Books.FindAsync(request.BookId);

            if (book == null) return NotFound("Sách không tồn tại.");
            if (book.Quantity <= 0) return BadRequest("Sách đã hết.");

            // Trừ số lượng tồn kho
            book.Quantity = book.Quantity - 1;

            // Lưu lại vào Database
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mượn thành công!", remaining = book.Quantity });
        }
    }

    // Class nhận dữ liệu từ Client gửi lên
    public class BorrowRequest
    {
        public string BookId { get; set; }
        public string StudentId { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}