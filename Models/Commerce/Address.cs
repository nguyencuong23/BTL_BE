using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Commerce
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; } = null!;

        [Required, StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = null!;

        [Required, StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string Line1 { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        public string? District { get; set; }

        [StringLength(100)]
        [Display(Name = "Tỉnh/Thành phố")]
        public string? Province { get; set; }

        [Display(Name = "Mặc định")]
        public bool IsDefault { get; set; } = false;

        public Users.User? User { get; set; }
    }
}

