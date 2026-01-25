using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Controllers
{
    public class ReadersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReadersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Readers
        public async Task<IActionResult> Index()
        {
            var readers = await _context.Users
                .Where(u => u.Role == 2)
                .ToListAsync();
            return View(readers);
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(int? id)
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
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Handle database errors (duplicate key, constraint violations, etc.)
                    ModelState.AddModelError("", "Không thể lưu dữ liệu. Có thể bị trùng tên đăng nhập hoặc vi phạm ràng buộc dữ liệu. Chi tiết: " + ex.InnerException?.Message);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
