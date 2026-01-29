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

        // Trang chính admin - Dashboard (Dữ liệu sẽ được load qua API)
        public IActionResult Index()
        {
            return View();
        }

        // 4. TRANG QUẢN LÝ NGƯỜI DÙNG
        // GET: /Admin/UserManagement
        public IActionResult UserManagement()
        {
            return View();
        }

        // 5. TRANG THỐNG KÊ
        // GET: /Admin/Statistics
        public IActionResult Statistics()
        {
            return View();
        }

        // 6. TRANG CÀI ĐẶT HỆ THỐNG
        // GET: /Admin/Settings
        public IActionResult Settings()
        {
            return View();
        }
    }
}