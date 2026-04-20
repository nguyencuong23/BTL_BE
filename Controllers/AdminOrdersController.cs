using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Commerce;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminOrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus? status, PaymentMethod? paymentMethod)
        {
            var q = _db.Orders.AsNoTracking().Include(o => o.User).AsQueryable();

            if (status.HasValue) q = q.Where(o => o.Status == status.Value);
            if (paymentMethod.HasValue) q = q.Where(o => o.PaymentMethod == paymentMethod.Value);

            var orders = await q.OrderByDescending(o => o.CreatedAt).Take(500).ToListAsync();
            ViewData["Status"] = status;
            ViewData["PaymentMethod"] = paymentMethod;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBankTransfer(int id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();

            if (order.PaymentMethod != PaymentMethod.BankTransfer)
            {
                TempData["ErrorMessage"] = "Đơn này không phải chuyển khoản.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = order.Status == OrderStatus.Pending ? OrderStatus.Confirmed : order.Status;
            order.ConfirmedAt ??= DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xác nhận chuyển khoản.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();

            order.Status = status;
            if (status == OrderStatus.Confirmed) order.ConfirmedAt ??= DateTime.Now;
            if (status == OrderStatus.Delivered) order.DeliveredAt ??= DateTime.Now;
            if (status == OrderStatus.Cancelled) order.CancelledAt ??= DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái đơn.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            if (order.Status == OrderStatus.Delivered)
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn đã giao.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Đơn đã bị hủy trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Restock
            var bookIds = order.Items.Select(i => i.BookId).Distinct().ToList();
            var books = await _db.Books.Where(b => bookIds.Contains(b.BookId)).ToListAsync();
            foreach (var it in order.Items)
            {
                var b = books.FirstOrDefault(x => x.BookId == it.BookId);
                if (b != null) b.Quantity += it.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã hủy đơn và hoàn kho.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

