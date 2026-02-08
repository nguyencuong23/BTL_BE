using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Display(Name = "Tên hệ thống")]
        public string SystemName { get; set; } = "Hệ thống Quản lý Thư viện Đại học Đại Nam";

        [Display(Name = "Email quản trị viên")]
        public string AdminEmail { get; set; } = "admin@thuvien.dainam.edu.vn";

        [Display(Name = "Số điện thoại liên hệ")]
        public string ContactPhone { get; set; } = "0243.123.4567";

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = "Hà Đông, Hà Nội";

        [Display(Name = "Bật chế độ bảo trì")]
        public bool MaintenanceMode { get; set; } = false;

        [Display(Name = "Thời gian mượn sách mặc định (ngày)")]
        public int DefaultLoanDays { get; set; } = 14;

        [Display(Name = "Tiền phạt mỗi ngày quá hạn (VNĐ)")]
        public decimal DailyFineAmount { get; set; } = 5000;

        [Display(Name = "Link Facebook")]
        public string FacebookUrl { get; set; } = "https://facebook.com/hongchucdangiu";

        [Display(Name = "Link Youtube")]
        public string YoutubeUrl { get; set; } = "https://www.youtube.com/@Chenny_Cute";

        [Display(Name = "Link Tiktok")]
        public string TiktokUrl { get; set; } = "https://tiktok.com/@chennysocute";
    }
}
