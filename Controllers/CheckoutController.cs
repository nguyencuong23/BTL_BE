using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Helpers;
using QuanLyThuVienTruongHoc.Models.Commerce;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using QuanLyThuVienTruongHoc.Services;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CartService _cart;
        private readonly SystemSettingsService _settings;

        public CheckoutController(ApplicationDbContext db, CartService cart, SystemSettingsService settings)
        {
            _db = db;
            _cart = cart;
            _settings = settings;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _cart.GetDetailedItemsAsync();
            if (items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var username = User.Identity?.Name;
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            var vm = new CheckoutViewModel
            {
                ReceiverName = user?.FullName ?? "",
                ReceiverPhone = user?.PhoneNumber ?? "",
                ShippingAddress = ""
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var items = await _cart.GetDetailedItemsAsync();
            if (items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid) return View("Index", model);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            // Re-check stock with tracked entities
            var ids = items.Select(i => i.Book.BookId).Distinct().ToList();
            var books = await _db.Books.Where(b => ids.Contains(b.BookId)).ToListAsync();
            foreach (var it in items)
            {
                var b = books.First(x => x.BookId == it.Book.BookId);
                if (!b.IsPublished)
                {
                    ModelState.AddModelError("", $"Sách '{b.Title}' hiện không còn được bán.");
                    return View("Index", model);
                }
                if (b.Quantity < it.Quantity)
                {
                    ModelState.AddModelError("", $"Sách '{b.Title}' không đủ tồn kho (còn {b.Quantity}).");
                    return View("Index", model);
                }
            }

            var shippingFee = await GetShippingFeeAsync(items.Sum(x => x.UnitPrice * x.Quantity));
            var subtotal = items.Sum(x => x.UnitPrice * x.Quantity);
            var discount = 0m;
            var total = subtotal + shippingFee - discount;

            var now = DateTime.Now;
            var order = new Order
            {
                UserId = user.Id,
                OrderCode = $"BP{now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}",
                Status = OrderStatus.Pending,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == PaymentMethod.BankTransfer ? PaymentStatus.PendingConfirmation : PaymentStatus.Unpaid,
                ReceiverName = model.ReceiverName,
                ReceiverPhone = model.ReceiverPhone,
                ShippingAddress = model.ShippingAddress,
                Note = model.Note,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                Discount = discount,
                Total = total,
                BankTransferReference = model.PaymentMethod == PaymentMethod.BankTransfer ? model.BankTransferReference : null,
                CreatedAt = now
            };

            foreach (var it in items)
            {
                order.Items.Add(new OrderItem
                {
                    BookId = it.Book.BookId,
                    UnitPrice = it.UnitPrice,
                    Quantity = it.Quantity,
                    LineTotal = it.UnitPrice * it.Quantity
                });
            }

            // Reduce stock
            foreach (var it in items)
            {
                var b = books.First(x => x.BookId == it.Book.BookId);
                b.Quantity -= it.Quantity;
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _cart.Clear();

            return RedirectToAction(nameof(Success), new { id = order.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _db.Orders.AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        private async Task<decimal> GetShippingFeeAsync(decimal subtotal)
        {
            var feeStr = await _settings.GetValueAsync(SettingsKeys.DefaultShippingFee, "30000");
            var freeStr = await _settings.GetValueAsync(SettingsKeys.FreeShippingThreshold, "300000");

            _ = decimal.TryParse(feeStr, out var fee);
            _ = decimal.TryParse(freeStr, out var threshold);

            if (threshold > 0 && subtotal >= threshold) return 0;
            return fee > 0 ? fee : 0;
        }
    }
}

