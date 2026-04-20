using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Commerce
{
    public class CartItem
    {
        [Required]
        public string BookId { get; set; } = null!;

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();
    }
}

