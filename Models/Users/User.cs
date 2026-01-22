using QuanLyThuVienTruongHoc.Models.Library;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Users
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Required]
        public int Role { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, double.MaxValue)]
        public decimal TotalFine { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Loan>? Loans { get; set; }
    }
}
