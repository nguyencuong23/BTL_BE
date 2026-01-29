using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.System
{
    public class Setting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
    }
}
