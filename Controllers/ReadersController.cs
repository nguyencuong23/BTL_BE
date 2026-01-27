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
    public class ReadersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReadersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Readers
        public async Task<IActionResult> Index(int? status, string sortOrder)
        {
            var usersQuery = _context.Users.Where(u => u.Role == 2).Include(u => u.Loans).AsQueryable();

            // Filter by Status
            if (status.HasValue)
            {
                // status: 1=Active, 0=Locked (mapping logic: IsActive=true -> 1, false -> 0)
                // UI dropdown: 1=Hoạt động, 0=Khóa
                bool isActive = status.Value == 1;
                usersQuery = usersQuery.Where(u => u.IsActive == isActive);
                ViewData["CurrentStatus"] = status.Value;
            }

            // Sort
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSort"] = sortOrder;

            usersQuery = sortOrder switch
            {
                "name_desc" => usersQuery.OrderByDescending(u => u.FullName).ThenBy(u => u.Id),
                // Cũ nhất: Xếp theo Ngày tạo tăng dần. Nếu trùng ngày => ID nhỏ hơn đứng trước (nhập trước).
                "Date" => usersQuery.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
                // Mới nhất: Xếp theo Ngày tạo giảm dần. Nếu trùng ngày => ID lớn hơn đứng trước (nhập sau).
                "date_desc" => usersQuery.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id),
                _ => usersQuery.OrderBy(u => u.FullName).ThenBy(u => u.Id),
            };

            return View(await usersQuery.ToListAsync());
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Loans)
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == 2);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Readers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Readers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentCode,Username,PasswordHash,FullName,Email,PhoneNumber,IsActive,TotalFine")] User user)
        {
            // Tự động set Role = 2 cho Sinh viên
            user.Role = 2;
            user.CreatedAt = DateTime.Now;

            // Remove Role from ModelState vì chúng ta set manual
            ModelState.Remove("Role");

            // StudentCode is required for Readers
            if (string.IsNullOrWhiteSpace(user.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "Mã sinh viên không được để trống");
            }

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

            if (await _context.Users.AnyAsync(u => u.StudentCode == user.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại");
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
                    TempData["SuccessMessage"] = "Thêm sinh viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", DbErrorHelper.TranslateDbError(ex));
                }
            }
            return View(user);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != 2)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Readers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentCode,Username,PasswordHash,FullName,Email,PhoneNumber,IsActive,TotalFine,CreatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Đảm bảo Role vẫn là 2
            user.Role = 2;

            // Remove Role from ModelState
            ModelState.Remove("Role");
            // Remove PasswordHash from ModelState - we preserve it from existing user
            ModelState.Remove("PasswordHash");

            // StudentCode is required for Readers
            if (string.IsNullOrWhiteSpace(user.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "Mã sinh viên không được để trống");
            }

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
                    TempData["SuccessMessage"] = "Cập nhật sinh viên thành công!";
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
                    ModelState.AddModelError("", DbErrorHelper.TranslateDbError(ex));
                }
            }
            return View(user);
        }

        // GET: Readers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == 2);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Readers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Role == 2)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sinh viên thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Readers/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != 2) return NotFound();

            var viewModel = new ChangePasswordViewModel { UserId = user.Id };
            return View(viewModel);
        }

        // POST: Readers/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null || user.Role != 2)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên.";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFine(int userId, decimal amount)
        {
            var user = await _context.Users.Include(u => u.Loans).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Role != 2)
            {
                return NotFound();
            }

            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Cộng dồn vào PaidAmount
            user.PaidAmount += amount;

            // Tính lại TotalFine
            var totalLoanFines = user.Loans.Sum(l => l.Fine);
            user.TotalFine = totalLoanFines - user.PaidAmount;

            // Mở khóa nếu nợ <= 50,000
            if (user.TotalFine <= 50000 && !user.IsActive)
            {
                user.IsActive = true; 
                // Note: user.IsActive could have been false manually, but here we assume it was due to fine
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thanh toán {amount:N0} đ. Số nợ còn lại: {user.TotalFine:N0} đ";
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
