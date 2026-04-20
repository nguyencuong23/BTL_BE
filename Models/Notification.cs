using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models
{
    public enum NotificationType
    {
        // Legacy (library)
        NearDue = 1,
        Overdue = 2,
        System = 3,
        /// <summary>Legacy: Tiền phạt gần ngưỡng (không dùng ở shop).</summary>
        FineWarning = 4,

        // E-commerce
        PaymentPending = 5,
        OrderStatus = 6
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
        /// Id thực thể liên quan (OrderId, ...).
        /// </summary>
        public int? RelatedEntityId { get; set; }

        // Navigation
        public Users.User? User { get; set; }
    }
}
