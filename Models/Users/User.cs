using QuanLyThuVienTruongHoc.Models.Library;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Users
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Mã sinh viên")]
        [StringLength(20, ErrorMessage = "Mã sinh viên không được vượt quá 20 ký tự")]
        public string? StudentCode { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ (VD: example@domain.com)")]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Email phải đúng định dạng (VD: example@domain.com)")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678)")]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Vai trò")]
        [Range(1, 2, ErrorMessage = "Vai trò không hợp lệ")]
        public int Role { get; set; } // 1-Admin, 2-User

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Tổng tiền phạt")]
        [Range(0, 999999999, ErrorMessage = "Tổng tiền phạt không hợp lệ")]
        public decimal TotalFine { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}