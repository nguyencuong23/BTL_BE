using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string sortOrder)
        {
            var categoriesQuery = _context.Categories.AsQueryable();

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentSort"] = sortOrder;

            categoriesQuery = sortOrder switch
            {
                "name_desc" => categoriesQuery.OrderByDescending(c => c.Name).ThenBy(c => c.CategoryId),
                _ => categoriesQuery.OrderBy(c => c.Name).ThenBy(c => c.CategoryId),
            };

            return View(await categoriesQuery.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm thể loại '{category.Name}' (Mã: {category.CategoryId}) thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var errorMsg = ex.InnerException?.Message ?? ex.Message;
                    if (errorMsg.Contains("PRIMARY KEY") || errorMsg.Contains("duplicate"))
                    {
                        ModelState.AddModelError("CategoryId", $"Mã thể loại '{category.CategoryId}' đã tồn tại. Vui lòng chọn mã khác.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể thêm thể loại. Lỗi cơ sở dữ liệu: " + errorMsg);
                    }
                }
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CategoryId,Name,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật thể loại '{category.Name}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thể loại cần cập nhật. Có thể đã bị xóa.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    var errorMsg = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "Không thể cập nhật thể loại. Lỗi: " + errorMsg);
                }
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thể loại cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                var categoryName = category.Name;
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa thể loại '{categoryName}' thành công!";
            }
            catch (DbUpdateException ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                if (errorMsg.Contains("REFERENCE") || errorMsg.Contains("FOREIGN KEY"))
                {
                    TempData["ErrorMessage"] = "Không thể xóa thể loại này vì đang được sử dụng bởi các sách khác.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa thể loại. Lỗi: " + errorMsg;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
