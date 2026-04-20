using QuanLyThuVienTruongHoc.Models.Library;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class CartLineViewModel
    {
        public Book Book { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartLineViewModel> Lines { get; set; } = new();
        public decimal Subtotal => Lines.Sum(x => x.LineTotal);
    }
}

