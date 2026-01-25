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
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
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