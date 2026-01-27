using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index(int? status, string sortOrder)
        {
            var loansQuery = _context.Loans.Include(l => l.Book).Include(l => l.User).AsQueryable();

            if (status.HasValue)
            {
                // Logic lọc đặc biệt cho Status
                if (status.Value == 3) // Quá hạn
                {
                     // Lọc: Status là Quá hạn HOẶC (Đang mượn VÀ Hạn trả < Hôm nay)
                     loansQuery = loansQuery.Where(l => l.Status == LoanStatus.QuaHan 
                                                     || (l.Status == LoanStatus.DangMuon && l.DueDate < DateTime.Now));
                }
                else
                {
                    loansQuery = loansQuery.Where(l => (int)l.Status == status.Value);
                }
                
                ViewData["CurrentStatus"] = status.Value;
            }

            // Sắp xếp: Ưu tiên theo nhóm trạng thái: Đang mượn (1) -> Quá hạn (2) -> Đã trả (3)
            // Lưu ý: Enum giá trị gốc: DangMuon=1, DaTra=2, QuaHan=3
            // Cần map: DangMuon(1)->1, QuaHan(3)->2, DaTra(2)->3
            
            // Logic sắp xếp phụ
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["BookSortParm"] = sortOrder == "Book" ? "book_desc" : "Book";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSort"] = sortOrder;

            loansQuery = sortOrder switch
            {
                "name_desc" => loansQuery.OrderByDescending(l => l.User.FullName)
                                         .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
                "Book" => loansQuery.OrderBy(l => l.Book.Title)
                                    .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
                "book_desc" => loansQuery.OrderByDescending(l => l.Book.Title)
                                         .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
                "Date" => loansQuery.OrderBy(l => l.BorrowDate)
                                    .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
                "date_desc" => loansQuery.OrderByDescending(l => l.BorrowDate)
                                         .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
                _ => loansQuery.OrderBy(l => l.User.FullName) // Mặc định A-Z
                               .ThenBy(l => l.Status == LoanStatus.DangMuon ? 1 : l.Status == LoanStatus.QuaHan ? 2 : 3),
            };

            return View(await loansQuery.ToListAsync());
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LoanId == id);
            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            // Chỉ hiển thị User là Reader (Role=2), không bị khóa, và mượn < 3 quyển
            // Logic phức tạp hơn nên lọc ở server memory hoặc query
            // Tuy nhiên SelectList cần list phẳng.
            // Tốt nhất là lọc trong query
            var availableUsers = _context.Users
                .Where(u => u.Role == 2 && u.IsActive && u.Loans.Count(l => l.Status == LoanStatus.DangMuon || l.Status == LoanStatus.QuaHan) < 3)
                .Select(u => new { u.Id, FullName = u.FullName + " (User Code: " + u.StudentCode + ")" });
            
            ViewData["UserId"] = new SelectList(availableUsers, "Id", "FullName");

            // Chỉ hiển thị sách còn số lượng > 0
            var availableBooks = _context.Books
                .Where(b => b.Quantity > 0)
                .Select(b => new { b.BookId, Title = b.Title + " (SL: " + b.Quantity + ")" });

            ViewData["BookId"] = new SelectList(availableBooks, "BookId", "Title");
            return View();
        }

        // POST: Loans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoanId,UserId,BookId,BorrowDate,ReturnDate,Status")] Loan loan)
        {
            // Tự động tính hạn trả = ngày mượn + 14 ngày
            loan.DueDate = loan.BorrowDate.AddDays(14);
            // Tính tiền phạt ngay khi tạo nếu đã có ngày trả
            if (loan.ReturnDate.HasValue && loan.ReturnDate.Value > loan.DueDate)
            {
                int overdueDays = (loan.ReturnDate.Value - loan.DueDate).Days;
                loan.Fine = overdueDays * 5000;
            }
            else
            {
                loan.Fine = 0;
            }

            // Xóa lỗi validation cho DueDate và Fine vì chúng ta tự gán giá trị
            ModelState.Remove("DueDate");
            ModelState.Remove("Fine");

            // Kiểm tra Business Rules
            var user = await _context.Users.Include(u => u.Loans).FirstOrDefaultAsync(u => u.Id == loan.UserId);
            var book = await _context.Books.FindAsync(loan.BookId);

            if (user == null || book == null)
            {
                ModelState.AddModelError("", "Người dùng hoặc Sách không tồn tại.");
            }
            else
            {
                // 1. Chỉ Reader (Role=2) mới được mượn
                if (user.Role != 2)
                {
                    ModelState.AddModelError("UserId", "Chỉ độc giả (Sinh viên/Giảng viên) mới được mượn sách.");
                }

                // 2. Kiểm tra nợ > 50k
                if (user.TotalFine > 50000)
                {
                    ModelState.AddModelError("UserId", $"Độc giả này đang nợ {user.TotalFine:N0} VNĐ. Vui lòng đóng phạt trước khi mượn.");
                }

                // 3. Kiểm tra số lượng sách đang mượn < 3
                // Lưu ý: user.Loans đã include ở trên
                int currentLoans = user.Loans.Count(l => l.Status == LoanStatus.DangMuon || l.Status == LoanStatus.QuaHan);
                if (currentLoans >= 3)
                {
                    ModelState.AddModelError("UserId", "Độc giả này đã mượn tối đa 3 cuốn sách.");
                }

                // 4. Kiểm tra số lượng sách trong kho > 0
                if (book.Quantity <= 0)
                {
                    ModelState.AddModelError("BookId", "Sách này đã hết hàng.");
                }

                // 5. [Validation] Ngày trả thực tế phải >= Ngày mượn
                if (loan.ReturnDate.HasValue && loan.ReturnDate.Value < loan.BorrowDate)
                {
                     ModelState.AddModelError("ReturnDate", "Ngày trả thực tế không được nhỏ hơn ngày mượn.");
                }
            }

            if (ModelState.IsValid)
            {
                // Trừ số lượng sách
                book.Quantity -= 1;
                _context.Books.Update(book);

                _context.Add(loan);
                await _context.SaveChangesAsync();


                // Cập nhật tổng tiền phạt cho User
                await UpdateUserTotalFine(loan.UserId);
                TempData["SuccessMessage"] = "Tạo phiếu mượn thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", loan.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
            return View(loan);
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", loan.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
            return View(loan);
        }

        // POST: Loans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LoanId,UserId,BookId,BorrowDate,ReturnDate,Status")] Loan loan)
        {
            if (id != loan.LoanId)
            {
                return NotFound();
            }

            // Tự động tính hạn trả = ngày mượn + 14 ngày
            loan.DueDate = loan.BorrowDate.AddDays(14);
            
            // Cập nhật trạng thái dựa trên ngày trả
            if (loan.ReturnDate.HasValue)
            {
                loan.Status = LoanStatus.DaTra;
            }
            else if (loan.DueDate < DateTime.Today)
            {
                loan.Status = LoanStatus.QuaHan;
            }
            else
            {
                loan.Status = LoanStatus.DangMuon;
            }

            // Tính tiền phạt
            if (loan.ReturnDate.HasValue && loan.ReturnDate.Value > loan.DueDate)
            {
                // Muộn 1 ngày phạt 5000
                int overdueDays = (loan.ReturnDate.Value - loan.DueDate).Days;
                loan.Fine = overdueDays * 5000;
            }
            else
            {
                loan.Fine = 0;
            }

            // [Validation] Ngày trả thực tế phải >= Ngày mượn
            if (loan.ReturnDate.HasValue && loan.ReturnDate.Value < loan.BorrowDate)
            {
                    ModelState.AddModelError("ReturnDate", "Ngày trả thực tế không được nhỏ hơn ngày mượn.");
            }

            // Xóa lỗi validation
            ModelState.Remove("DueDate");
            ModelState.Remove("Fine");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update Logic Kho:
                    // Lấy trạng thái cũ của Loan từ DB (AsNoTracking để không conflict tracking)
                    var oldLoan = await _context.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.LoanId == id);
                    if (oldLoan != null)
                    {
                        var bookToUpdate = await _context.Books.FindAsync(loan.BookId);
                        if (bookToUpdate != null)
                        {
                            // Case 1: Đang mượn/Quá hạn -> Đã trả : Tăng số lượng (+1)
                            bool isReturning = (oldLoan.Status == LoanStatus.DangMuon || oldLoan.Status == LoanStatus.QuaHan) 
                                               && (loan.Status == LoanStatus.DaTra);
                            
                            // Case 2: Đã trả -> Đang mượn/Quá hạn (Undo trả): Giảm số lượng (-1)
                            bool isUndoReturn = (oldLoan.Status == LoanStatus.DaTra) 
                                                && (loan.Status == LoanStatus.DangMuon || loan.Status == LoanStatus.QuaHan);

                            if (isReturning)
                            {
                                bookToUpdate.Quantity += 1;
                                _context.Books.Update(bookToUpdate);
                            }
                            else if (isUndoReturn)
                            {
                                // Chỉ giảm nếu còn > 0 để tránh âm (tùy nghiệp vụ, ở đây cứ trừ)
                                if (bookToUpdate.Quantity > 0) 
                                {
                                    bookToUpdate.Quantity -= 1;
                                    _context.Books.Update(bookToUpdate);
                                }
                            }
                        }
                    }

                    _context.Update(loan);
                    await _context.SaveChangesAsync();

                    // Cập nhật tổng tiền phạt cho User
                    await UpdateUserTotalFine(loan.UserId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loan.LoanId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật phiếu mượn thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", loan.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
            return View(loan);
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LoanId == id);
            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                _context.Loans.Remove(loan);
                
                // Nếu xóa phiếu đang mượn hoặc quá hạn (chưa trả) -> hoàn lại sách
                if (loan.Status == LoanStatus.DangMuon || loan.Status == LoanStatus.QuaHan)
                {
                    var book = await _context.Books.FindAsync(loan.BookId);
                    if (book != null)
                    {
                        book.Quantity += 1;
                        _context.Books.Update(book);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (loan != null)
            {
               await UpdateUserTotalFine(loan.UserId);
            }

            TempData["SuccessMessage"] = "Xóa phiếu mượn thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.LoanId == id);
        }

        private async Task UpdateUserTotalFine(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var totalFine = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .SumAsync(l => l.Fine);
                
                // Cập nhật lại tổng nợ thực tế = Tổng phạt từ các phiếu - Tổng đã đóng
                user.TotalFine = totalFine - user.PaidAmount;

                // Tự động khóa tài khoản nếu nợ > 50,000
                if (user.TotalFine > 50000)
                {
                    user.IsActive = false;
                }
                // Nếu muốn tự động mở lại khi trả hết nợ thì uncomment dòng dưới,
                 // nhưng yêu cầu chỉ nói user bị khóa nếu > 50k. 
                 // Logic tốt nhất nên là: <= 50000 thì Active lại.
                else
                {
                     user.IsActive = true;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
