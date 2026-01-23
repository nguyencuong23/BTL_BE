using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Users;

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

        // 1. HIỂN THỊ DANH SÁCH SINH VIÊN
        // GET: /Admin/DanhSachSinhVien
        public async Task<IActionResult> DanhSachSinhVien()
        {
            var sinhViens = await _context.SinhViens
                                          .OrderBy(s => s.MaSinhVien)
                                          .ToListAsync();

            return View(sinhViens);
        }

        // 2. HIỂN THỊ FORM THÊM SINH VIÊN
        // GET: /Admin/ThemSinhVien
        [HttpGet]
        public IActionResult ThemSinhVien()
        {
            return View();
        }

        // 3. XỬ LÝ SUBMIT FORM THÊM SINH VIÊN
        // POST: /Admin/ThemSinhVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSinhVien(SinhVien model)
        {
            if (!ModelState.IsValid)
            {
                // Dữ liệu không hợp lệ => hiển thị lại form cùng lỗi
                return View(model);
            }

            _context.SinhViens.Add(model);
            await _context.SaveChangesAsync();

            // Lưu xong quay lại trang danh sách
            return RedirectToAction(nameof(DanhSachSinhVien));
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