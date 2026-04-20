using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVienTruongHoc.Models.Commerce
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string OrderCode { get; set; } = null!;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cod;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [Required, StringLength(100)]
        public string ReceiverName { get; set; } = null!;

        [Required, StringLength(15)]
        public string ReceiverPhone { get; set; } = null!;

        [Required, StringLength(300)]
        public string ShippingAddress { get; set; } = null!;

        [StringLength(500)]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(100)]
        public string? BankTransferReference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Users.User? User { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

