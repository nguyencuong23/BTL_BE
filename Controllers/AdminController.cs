using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Data;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.SystemSettingsService _settingsService;

        public AdminController(ApplicationDbContext context, Services.SystemSettingsService settingsService)
        {
            _context = context;
            _settingsService = settingsService;
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
        // 6. TRANG CÀI ĐẶT HỆ THỐNG
        // GET: /Admin/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var model = await _settingsService.GetSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(Models.ViewModels.SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _settingsService.UpdateSettingsAsync(model);
                TempData["Success"] = "Đã lưu cài đặt hệ thống thành công!";
                return RedirectToAction(nameof(Settings));
            }
            return View(model);
        }
    }
}