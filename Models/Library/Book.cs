using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVienTruongHoc.Models.Library
{
    public class Book
    {
        [Key]
        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(7, ErrorMessage = "Mã sách phải có dạng TL-NNNN (7 ký tự)")]
        [RegularExpression(@"^[A-Za-z]{2}-\d{4}$", ErrorMessage = "Mã sách phải đúng định dạng TL-NNNN (VD: IT-0001)")]
        [Display(Name = "Mã sách")]
        public string BookId { get; set; } = null!;

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(150, ErrorMessage = "Tác giả không được vượt quá 150 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = null!;

        [StringLength(150, ErrorMessage = "Nhà xuất bản không được vượt quá 150 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? Publisher { get; set; }

        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string? Isbn { get; set; }

        [Required(ErrorMessage = "Thể loại không được để trống")]
        [StringLength(2)]
        [Display(Name = "Thể loại")]
        public string CategoryId { get; set; } = null!;

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, 999999999, ErrorMessage = "Giá bán không hợp lệ")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; } = 0;

        [Range(0, 999999999, ErrorMessage = "Giá khuyến mãi không hợp lệ")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? SalePrice { get; set; }

        [StringLength(3000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(200)]
        [Display(Name = "Slug")]
        public string? Slug { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsPublished { get; set; } = true;

        [Required(ErrorMessage = "Năm xuất bản không được để trống")]
        [Display(Name = "Năm xuất bản")]
        [Range(1450, 9999, ErrorMessage = "Năm xuất bản không hợp lệ")]
        public int PublishYear { get; set; }

        [Required(ErrorMessage = "Tồn kho không được để trống")]
        [Display(Name = "Tồn kho")]
        [Range(0, 9999, ErrorMessage = "Số lượng phải từ 0 đến 9999")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Vị trí kệ không được vượt quá 50 ký tự")]
        [Display(Name = "Ghi chú kho (tuỳ chọn)")]
        public string? Location { get; set; }

        [StringLength(255)]
        [Display(Name = "Ảnh")]
        public string? ImagePath { get; set; }

        public Category? Category { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<Commerce.OrderItem> OrderItems { get; set; } = new List<Commerce.OrderItem>();
    }
}