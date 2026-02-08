using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models
{
    public enum NotificationType
    {
        NearDue = 1,
        Overdue = 2,
        System = 3,
        /// <summary>Tiền phạt gần ngưỡng 50k (sắp bị khóa tài khoản).</summary>
        FineWarning = 4
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = null!;

        [StringLength(500)]
        public string? Link { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Id phiên mượn (LoanId) hoặc thực thể liên quan.
        /// </summary>
        public int? RelatedEntityId { get; set; }

        // Navigation
        public Users.User? User { get; set; }
    }
}
