using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Helpers;
using QuanLyThuVienTruongHoc.Models.Commerce;

namespace QuanLyThuVienTruongHoc.Services
{
    public class CartService
    {
        private const string SessionKey = "SHOP_CART";
        private readonly IHttpContextAccessor _http;
        private readonly ApplicationDbContext _db;

        public CartService(IHttpContextAccessor http, ApplicationDbContext db)
        {
            _http = http;
            _db = db;
        }

        private ISession Session => _http.HttpContext!.Session;

        public Cart GetCart() => Session.GetJson<Cart>(SessionKey) ?? new Cart();

        private void SaveCart(Cart cart) => Session.SetJson(SessionKey, cart);

        public void Add(string bookId, int quantity = 1)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(x => x.BookId == bookId);
            if (item == null)
            {
                cart.Items.Add(new CartItem { BookId = bookId, Quantity = Math.Max(1, quantity) });
            }
            else
            {
                item.Quantity = Math.Min(999, item.Quantity + Math.Max(1, quantity));
            }

            SaveCart(cart);
        }

        public void Update(string bookId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(x => x.BookId == bookId);
            if (item == null) return;

            if (quantity <= 0)
                cart.Items.Remove(item);
            else
                item.Quantity = Math.Min(999, quantity);

            SaveCart(cart);
        }

        public void Remove(string bookId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(x => x.BookId == bookId);
            SaveCart(cart);
        }

        public void Clear() => Session.Remove(SessionKey);

        public async Task<IReadOnlyList<(Models.Library.Book Book, int Quantity, decimal UnitPrice)>> GetDetailedItemsAsync()
        {
            var cart = GetCart();
            if (cart.Items.Count == 0) return Array.Empty<(Models.Library.Book, int, decimal)>();

            var ids = cart.Items.Select(i => i.BookId).Distinct().ToList();
            var books = await _db.Books.AsNoTracking()
                .Where(b => ids.Contains(b.BookId) && b.IsPublished)
                .ToListAsync();

            var map = books.ToDictionary(b => b.BookId, b => b);
            var result = new List<(Models.Library.Book, int, decimal)>();

            foreach (var ci in cart.Items)
            {
                if (!map.TryGetValue(ci.BookId, out var book)) continue;
                var unit = book.SalePrice ?? book.Price;
                result.Add((book, ci.Quantity, unit));
            }

            return result;
        }
    }
}

