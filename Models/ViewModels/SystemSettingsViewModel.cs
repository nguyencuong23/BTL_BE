using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Display(Name = "Tên hệ thống")]
        public string SystemName { get; set; } = "BookPlanet - Thế giới của những người yêu sách";

        [Display(Name = "Email quản trị viên")]
        public string AdminEmail { get; set; } = "admin@thuvien.dainam.edu.vn";

        [Display(Name = "Số điện thoại liên hệ")]
        public string ContactPhone { get; set; } = "0243.123.4567";

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = "Hà Đông, Hà Nội";

        [Display(Name = "Bật chế độ bảo trì")]
        public bool MaintenanceMode { get; set; } = false;

        [Display(Name = "Phí ship mặc định (VNĐ)")]
        public decimal DefaultShippingFee { get; set; } = 30000;

        [Display(Name = "Ngưỡng miễn phí ship (VNĐ)")]
        public decimal FreeShippingThreshold { get; set; } = 300000;

        [Display(Name = "Ngân hàng")]
        public string BankName { get; set; } = "";

        [Display(Name = "Số tài khoản")]
        public string BankAccountNumber { get; set; } = "";

        [Display(Name = "Chủ tài khoản")]
        public string BankAccountName { get; set; } = "";

        [Display(Name = "Link Facebook")]
        public string FacebookUrl { get; set; } = "https://facebook.com/hongchucdangiu";

        [Display(Name = "Link Youtube")]
        public string YoutubeUrl { get; set; } = "https://www.youtube.com/@Chenny_Cute";

        [Display(Name = "Link Tiktok")]
        public string TiktokUrl { get; set; } = "https://tiktok.com/@chennysocute";
    }
}
