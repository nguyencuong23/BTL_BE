using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Library
{
    public class Book
    {
        [Key]
        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(7, ErrorMessage = "Mã sách phải có dạng TL-NNNN (7 ký tự)")]
        [RegularExpression(@"^[A-Za-z]{2}-\d{4}$", ErrorMessage = "Mã sách phải đúng định dạng TL-NNNN (VD: IT-0001)")]
        [Display(Name = "Mã sách")]
        public string BookId { get; set; } = null!; // TL-NNNN

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(150, ErrorMessage = "Tác giả không được vượt quá 150 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = null!;

        [Required(ErrorMessage = "Nhà xuất bản không được để trống")]
        [StringLength(150, ErrorMessage = "Nhà xuất bản không được vượt quá 150 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? Publisher { get; set; }

        [Required(ErrorMessage = "Thể loại không được để trống")]
        [StringLength(2)]
        [Display(Name = "Thể loại")]
        public string CategoryId { get; set; } = null!;

        [Required(ErrorMessage = "Năm xuất bản không được để trống")]
        [Display(Name = "Năm xuất bản")]
        [Range(1900, 2100, ErrorMessage = "Năm xuất bản phải từ 1900 đến 2100")]
        public int PublishYear { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Display(Name = "Số lượng")]
        [Range(0, 9999, ErrorMessage = "Số lượng phải từ 0 đến 9999")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vị trí kệ không được để trống")]
        [StringLength(50, ErrorMessage = "Vị trí kệ không được vượt quá 50 ký tự")]
        [Display(Name = "Vị trí kệ")]
        public string? Location { get; set; }
        
        [StringLength(255)]
        [Display(Name = "Ảnh")]
        public string? ImagePath { get; set; } // lưu đường dẫn ảnh upload

        // Navigation
        public Category? Category { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}