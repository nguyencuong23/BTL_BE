using QuanLyThuVienTruongHoc.Models.Library;
using System.ComponentModel.DataAnnotations;


namespace QuanLyThuVienTruongHoc.Models.Users
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        // Tài khoản đăng nhập
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        // Thông tin cá nhân
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        // Vai trò theo yêu cầu BTL
        // 1 = Admin (Thủ thư)
        // 2 = User (Độc giả / Sinh viên)
        [Required]
        public int Role { get; set; }

        // Trạng thái tài khoản
        // true = Hoạt động, false = Khóa
        public bool IsActive { get; set; } = true;

        // Tổng tiền phạt (dùng để khóa tài khoản nếu > 50,000 VNĐ)
        [Range(0, double.MaxValue)]
        public decimal TotalFine { get; set; } = 0;

        // Thời gian tạo tài khoản
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ: 1 User - nhiều Phiếu mượn
        public ICollection<Loan>? Loans { get; set; }
    }
}
