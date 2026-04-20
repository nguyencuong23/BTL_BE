using System.ComponentModel.DataAnnotations;
using QuanLyThuVienTruongHoc.Models.Commerce;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; } = null!;

        [Required, StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; } = null!;

        [Required, StringLength(300)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; } = null!;

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Note { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cod;

        [Display(Name = "Mã/ghi chú chuyển khoản (tuỳ chọn)")]
        [StringLength(100)]
        public string? BankTransferReference { get; set; }
    }
}

