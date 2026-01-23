using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyThuVienTruongHoc.Controllers
{
    /// <summary>
    /// Controller quản lý trang Thông tin cá nhân
    /// Yêu cầu đăng nhập mới được truy cập
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang thông tin cá nhân
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy UserId từ Claims (đã được set khi đăng nhập)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin user từ database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Map sang ViewModel
            var viewModel = new ProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role == 1 ? "Admin" : "User",
                CreatedAt = user.CreatedAt,
                Status = user.IsActive ? "Hoạt động" : "Đã khóa"
            };

            return View(viewModel);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu có lỗi validation, trả về view với model để hiển thị lỗi
                // Cần load lại thông tin chỉ đọc
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        model.Username = user.Username;
                        model.Role = user.Role == 1 ? "Admin" : "User";
                        model.CreatedAt = user.CreatedAt;
                        model.Status = user.IsActive ? "Hoạt động" : "Đã khóa";
                    }
                }
                return View("Index", model);
            }

            // Lấy UserId từ Claims
            var userIdClaim2 = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim2 == null || !int.TryParse(userIdClaim2.Value, out int userId2))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy user từ database
            var userToUpdate = await _context.Users.FindAsync(userId2);
            if (userToUpdate == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng";
                return RedirectToAction("Index");
            }

            // Cập nhật các trường được phép sửa
            userToUpdate.FullName = model.FullName;
            userToUpdate.Email = model.Email;
            userToUpdate.PhoneNumber = model.PhoneNumber;

            // Lưu vào database
            try
            {
                _context.Users.Update(userToUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }
    }
}
