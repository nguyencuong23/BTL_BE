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
    public class LibrariansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibrariansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Librarians
        public async Task<IActionResult> Index()
        {
            var librarians = await _context.Users
                .Where(u => u.Role == 1)
                .ToListAsync();
            return View(librarians);
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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
