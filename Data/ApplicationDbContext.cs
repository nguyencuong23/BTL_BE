using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Models.Library;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.TotalFine)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasPrecision(18, 0);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.PhoneNumber)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.StudentCode)
                .IsUnique()
                .HasFilter("[StudentCode] IS NOT NULL");
        }
    }
}
