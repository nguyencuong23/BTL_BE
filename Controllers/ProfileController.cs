using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Users;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace QuanLyThuVienTruongHoc.Controllers
{
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

            // Lấy thông tin user từ database kèm theo Loans
            var user = await _context.Users
                .Include(u => u.Loans)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tính toán
            var activeLoans = user.Loans.Count(l => l.Status == QuanLyThuVienTruongHoc.Models.Library.LoanStatus.DangMuon);
            var debt = user.TotalFine - user.PaidAmount;

            // Map sang ViewModel
            var viewModel = new ProfileViewModel
            {
                Username = user.Username,
                StudentCode = user.StudentCode,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role == 1 ? "Admin" : "User",
                CreatedAt = user.CreatedAt,
                Status = user.IsActive ? "Hoạt động" : "Đã khóa",
                LoanCount = activeLoans,
                Debt = debt
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

        /// <summary>
        /// Đổi mật khẩu cho user hiện tại
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            // Lấy UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            // Lấy user từ database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            // Validate password format (double check)
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.NewPassword))
            {
                return Json(new { success = false, message = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt." });
            }

            try
            {
                // Hash password
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, model.NewPassword);

                // Save to database
                _context.Entry(user).Property(u => u.PasswordHash).IsModified = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đổi mật khẩu: " + ex.Message });
            }
        }
    }
}
