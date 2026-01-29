using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVienTruongHoc.Models.Library
{
    [Table("Shelves")]
    public class Shelf
    {
        [Key]
        public int ShelfId { get; set; }

        [Required(ErrorMessage = "Tên kệ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên kệ không được vượt quá 100 ký tự")]
        [Display(Name = "Tên kệ")]
        public string Name { get; set; } = null!; // Thêm = null! để tránh warning

        [StringLength(200, ErrorMessage = "Mô tả vị trí không được quá dài")]
        [Display(Name = "Vị trí")]
        public string? Location { get; set; } // Ví dụ: Tầng 1, Phòng 101

        // Quan hệ: Một kệ chứa nhiều sách
        public virtual ICollection<Book> Books { get; set; } = new List<Book>(); // Khởi tạo List rỗng
    }
}