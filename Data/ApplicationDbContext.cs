using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<User> Users { get; set; }
        // DbSet ở đây, ví dụ:
        // public DbSet<User> Users { get; set; }
    }
}