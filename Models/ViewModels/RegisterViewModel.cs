using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
