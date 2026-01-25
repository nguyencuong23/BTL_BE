using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Data;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính admin (bạn đã có)
        public IActionResult Index()
        {
            return View();
        }

        // 4. TRANG QUẢN LÝ NGƯỜI DÙNG (Tĩnh - mẫu)
        // GET: /Admin/UserManagement
        public IActionResult UserManagement()
        {
            return View();
        }

        // 5. TRANG THỐNG KÊ (Tĩnh - mẫu)
        // GET: /Admin/Statistics
        public IActionResult Statistics()
        {
            return View();
        }

        // 6. TRANG CÀI ĐẶT HỆ THỐNG (Tĩnh - mẫu)
        // GET: /Admin/Settings
        public IActionResult Settings()
        {
            return View();
        }
    }
}