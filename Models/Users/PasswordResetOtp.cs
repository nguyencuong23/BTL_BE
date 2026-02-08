using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Users
{
    public class PasswordResetOtp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string OtpHash { get; set; } = null!;

        [Required]
        public DateTime ExpireAt { get; set; }

        [Required]
        public int AttemptCount { get; set; } = 0;

        [Required]
        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
