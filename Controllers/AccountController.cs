using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Users;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyThuVienTruongHoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Register removed - users are now created via Admin panel only

        [HttpGet]
        public IActionResult Login(bool locked = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            if (locked)
            {
                ViewBag.Locked = true;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            // Case 1: Tài khoản không tồn tại
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác");
                return View(model);
            }

            // Case 2: Tài khoản bị khóa
            if (!user.IsActive)
            {
                 // return RedirectToAction("Locked"); // OLD
                 ViewBag.Locked = true;
                 return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password
            );

            // Case 3: Sai mật khẩu
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role == 1 ? "Admin" : "User")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            if (model.RememberMe)
            {
                authProperties.ExpiresUtc = DateTime.UtcNow.AddDays(30);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties
            );

            return RedirectToAction("Index", "Client");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MaintenanceLogin()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MaintenanceLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác");
                return View(model);
            }

            if (!user.IsActive)
            {
                 ViewBag.Locked = true;
                 return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác");
                return View(model);
            }

            // Chỉ cho phép Admin đăng nhập ở đây
            if (user.Role != 1) 
            {
                ModelState.AddModelError("", "Chỉ Quản trị viên mới được phép đăng nhập khi bảo trì.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = model.RememberMe };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties
            );

            return RedirectToAction("Index", "Admin"); 
        }

        [HttpGet]
        [Route("/Maintenance")]
        public IActionResult Maintenance()
        {
            return View();
        }

        // ==================== FORGOT PASSWORD ACTIONS ====================

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Email không hợp lệ" });
            }

            // Check email tồn tại
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Json(new { success = false, message = "Email không tồn tại trên hệ thống" });
            }

            try
            {
                // Tạo OTP
                var otpService = HttpContext.RequestServices.GetRequiredService<Services.IOtpService>();
                var emailSender = HttpContext.RequestServices.GetRequiredService<Services.IEmailSender>();

                var otp = await otpService.CreateOtpAsync(model.Email);

                // Gửi email
                await emailSender.SendOtpEmailAsync(model.Email, otp, user.FullName);

                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var otpService = HttpContext.RequestServices.GetRequiredService<Services.IOtpService>();

            // Validate OTP
            var (isValid, errorMessage) = await otpService.ValidateOtpAsync(model.Email, model.Otp);
            if (!isValid)
            {
                return Json(new { success = false, message = errorMessage });
            }

            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Json(new { success = false, message = "Email không tồn tại trên hệ thống" });
            }

            // Update password
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);

            await _context.SaveChangesAsync();

            // Mark OTP as used
            await otpService.MarkOtpAsUsedAsync(model.Email);

            return Json(new { success = true, message = "Đổi mật khẩu thành công" });
        }
    }
}
