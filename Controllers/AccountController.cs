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

    }
}
