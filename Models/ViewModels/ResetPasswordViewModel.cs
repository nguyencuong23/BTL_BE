using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "OTP không được để trống")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải là 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string Otp { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
