using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Helpers;
using QuanLyThuVienTruongHoc.Models.Users;
using QuanLyThuVienTruongHoc.Models.ViewModels;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LibrariansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibrariansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Librarians
        public async Task<IActionResult> Index(string sortOrder)
        {
            var usersQuery = _context.Users.Where(u => u.Role == 1).AsQueryable();

            // Sort
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSort"] = sortOrder;

            usersQuery = sortOrder switch
            {
                "name_desc" => usersQuery.OrderByDescending(u => u.FullName).ThenBy(u => u.Id),
                "Date" => usersQuery.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
                "date_desc" => usersQuery.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id),
                _ => usersQuery.OrderBy(u => u.FullName).ThenBy(u => u.Id),
            };

            return View(await usersQuery.ToListAsync());
        }

        // GET: Librarians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == 1);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Librarians/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Librarians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,PasswordHash,FullName,Email,PhoneNumber,IsActive")] User user)
        {
            // Tự động set Role = 1, StudentCode = null, TotalFine = 0
            user.Role = 1;
            user.StudentCode = null;
            user.TotalFine = 0;
            user.CreatedAt = DateTime.Now;

            // Remove các field không trong form khỏi ModelState
            ModelState.Remove("Role");
            ModelState.Remove("StudentCode");
            ModelState.Remove("TotalFine");

            // Server-side format validation
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
                if (!emailRegex.IsMatch(user.Email))
                {
                    ModelState.AddModelError("Email", "Email phải đúng định dạng (VD: example@domain.com)");
                }
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^(0|\+84)\d{9,10}$");
                if (!phoneRegex.IsMatch(user.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678)");
                }
            }

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
                if (!passwordRegex.IsMatch(user.PasswordHash))
                {
                    ModelState.AddModelError("PasswordHash", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt");
                }
            }

            // Server-side uniqueness validation
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            if (!string.IsNullOrEmpty(user.Email) && await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber) && await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được sử dụng");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash password before saving
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        var hasher = new PasswordHasher<User>();
                        user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm thủ thư thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", DbErrorHelper.TranslateDbError(ex));
                }
            }
            return View(user);
        }

        // GET: Librarians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != 1)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Librarians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,PasswordHash,FullName,Email,PhoneNumber,IsActive,CreatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Đảm bảo Role vẫn là 1, StudentCode = null, TotalFine = 0
            user.Role = 1;
            user.StudentCode = null;
            user.TotalFine = 0;

            // Remove các field không trong form khỏi ModelState
            ModelState.Remove("Role");
            ModelState.Remove("StudentCode");
            ModelState.Remove("TotalFine");
            // Remove PasswordHash - we preserve it from existing user
            ModelState.Remove("PasswordHash");

            // Server-side format validation
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
                if (!emailRegex.IsMatch(user.Email))
                {
                    ModelState.AddModelError("Email", "Email phải đúng định dạng (VD: example@domain.com)");
                }
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^(0|\+84)\d{9,10}$");
                if (!phoneRegex.IsMatch(user.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678)");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing user to preserve password
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (existingUser != null)
                    {
                        user.PasswordHash = existingUser.PasswordHash;
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thủ thư thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Handle database errors
                    ModelState.AddModelError("", "Không thể cập nhật dữ liệu. Có thể bị trùng tên đăng nhập hoặc vi phạm ràng buộc dữ liệu. Chi tiết: " + ex.InnerException?.Message);
                }
            }
            return View(user);
        }

        // GET: Librarians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == 1);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Librarians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Role == 1)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thủ thư thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Librarians/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != 1) return NotFound();

            var viewModel = new ChangePasswordViewModel { UserId = user.Id };
            return View(viewModel);
        }

        // POST: Librarians/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null || user.Role != 1)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thủ thư.";
                return RedirectToAction(nameof(Index));
            }

            // Update password (admin feature - no current password check)
            // Validate strict password
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);

            try
            {
                // Explicitly mark PasswordHash as modified
                _context.Entry(user).Property(u => u.PasswordHash).IsModified = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi đổi mật khẩu: " + ex.Message);
                return View(model);
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
