using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Library
{
    public class Category
    {
        [Key]
        [Required(ErrorMessage = "Mã thể loại không được để trống")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Mã thể loại phải đúng 2 ký tự (VD: IT, VH, KT)")]
        [Display(Name = "Mã thể loại")]
        public string CategoryId { get; set; } = null!; // 2 ký tự

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự")]
        [Display(Name = "Tên thể loại")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}