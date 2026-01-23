using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Thông tin cá nhân
    /// Dùng để hiển thị và cập nhật thông tin người dùng
    /// </summary>
    public class ProfileViewModel
    {
        // Thông tin chỉ đọc (không cho sửa)
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Display(Name = "Vai trò")]
        public string Role { get; set; } = null!;

        [Display(Name = "Ngày tạo tài khoản")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = null!;

        // Thông tin có thể chỉnh sửa
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
    }
}
