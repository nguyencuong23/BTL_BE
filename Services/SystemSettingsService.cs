using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Helpers;
using QuanLyThuVienTruongHoc.Models.System;
using QuanLyThuVienTruongHoc.Models.ViewModels;

namespace QuanLyThuVienTruongHoc.Services
{
    public class SystemSettingsService
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = "")
        {
            var setting = await _context.Settings.FindAsync(key);
            return setting?.Value ?? defaultValue;
        }

        public async Task SetValueAsync(string key, string value, string? description = null)
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value ?? "", Description = description };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value ?? "";
                if (description != null) setting.Description = description;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<SystemSettingsViewModel> GetSettingsAsync()
        {
            var settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            string Get(string key, string def) => settings.ContainsKey(key) ? settings[key] : def;

            return new SystemSettingsViewModel
            {
                SystemName = Get(SettingsKeys.SystemName, "Hệ thống Quản lý Thư viện Đại học Đại Nam"),
                AdminEmail = Get(SettingsKeys.AdminEmail, "admin@thuvien.dainam.edu.vn"),
                ContactPhone = Get(SettingsKeys.ContactPhone, "0243.123.4567"),
                Address = Get(SettingsKeys.Address, "Hà Đông, Hà Nội"),
                MaintenanceMode = bool.TryParse(Get(SettingsKeys.MaintenanceMode, "false"), out var m) && m,
                DefaultLoanDays = int.TryParse(Get(SettingsKeys.DefaultLoanDays, "14"), out var d) ? d : 14,
                DailyFineAmount = decimal.TryParse(Get(SettingsKeys.DailyFineAmount, "5000"), out var f) ? f : 5000,
                FacebookUrl = Get(SettingsKeys.FacebookUrl, "https://facebook.com/hongchucdangiu"),
                YoutubeUrl = Get(SettingsKeys.YoutubeUrl, "https://www.youtube.com/@Chenny_Cute"),
                TiktokUrl = Get(SettingsKeys.TiktokUrl, "https://tiktok.com/@chennysocute")
            };
        }

        public async Task UpdateSettingsAsync(SystemSettingsViewModel model)
        {
            await SetValueAsync(SettingsKeys.SystemName, model.SystemName, "Tên hệ thống hiển thị");
            await SetValueAsync(SettingsKeys.AdminEmail, model.AdminEmail, "Email quản trị");
            await SetValueAsync(SettingsKeys.ContactPhone, model.ContactPhone, "Số điện thoại liên hệ");
            await SetValueAsync(SettingsKeys.Address, model.Address, "Địa chỉ thư viện");
            await SetValueAsync(SettingsKeys.MaintenanceMode, model.MaintenanceMode.ToString(), "Trạng thái bảo trì");
            await SetValueAsync(SettingsKeys.DefaultLoanDays, model.DefaultLoanDays.ToString(), "Thời gian mượn mặc định");
            await SetValueAsync(SettingsKeys.DailyFineAmount, model.DailyFineAmount.ToString(), "Tiền phạt quá hạn mỗi ngày");
            await SetValueAsync(SettingsKeys.FacebookUrl, model.FacebookUrl, "Link Facebook");
            await SetValueAsync(SettingsKeys.YoutubeUrl, model.YoutubeUrl, "Link Youtube");
            await SetValueAsync(SettingsKeys.TiktokUrl, model.TiktokUrl, "Link Tiktok");
        }
    }
}
