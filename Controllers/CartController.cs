using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using QuanLyThuVienTruongHoc.Services;

namespace QuanLyThuVienTruongHoc.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cart;

        public CartController(CartService cart)
        {
            _cart = cart;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _cart.GetDetailedItemsAsync();
            var vm = new CartViewModel
            {
                Lines = items.Select(x => new CartLineViewModel
                {
                    Book = x.Book,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(string bookId, int quantity = 1, string? returnUrl = null)
        {
            _cart.Add(bookId, quantity);
            if (!string.IsNullOrWhiteSpace(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(string bookId, int quantity)
        {
            _cart.Update(bookId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string bookId)
        {
            _cart.Remove(bookId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear();
            return RedirectToAction(nameof(Index));
        }
    }
}

