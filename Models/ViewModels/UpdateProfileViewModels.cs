using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class UpdateProfileVM
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
    }
}
