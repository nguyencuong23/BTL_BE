using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuanLyThuVienTruongHoc.Models.Library;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // đảm bảo đã migrate
        await db.Database.MigrateAsync();

        // đã có dữ liệu thì không seed nữa
        if (await db.Categories.AnyAsync() || await db.Books.AnyAsync() || await db.Users.AnyAsync() || await db.Loans.AnyAsync())
            return;

        var rnd = new Random(20260125);

        // =====================
        // 1) CATEGORIES (5)
        // =====================
        var categories = new List<Category>
        {
            new() { CategoryId = "IT", Name = "Công nghệ thông tin", Description = "Sách CNTT" },
            new() { CategoryId = "VH", Name = "Văn học", Description = "Tiểu thuyết, truyện ngắn..." },
            new() { CategoryId = "KT", Name = "Kinh tế", Description = "Quản trị, tài chính..." },
            new() { CategoryId = "KH", Name = "Khoa học", Description = "Toán, Lý, Hóa..." },
            new() { CategoryId = "NN", Name = "Ngoại ngữ", Description = "Anh, Nhật, Hàn..." }
        };
        db.Categories.AddRange(categories);

        // =====================
        // 2) USERS (30 độc giả + 1 admin)
        // =====================
        // Sử dụng PasswordHasher để hash password
        var hasher = new PasswordHasher<User>();
        string Hash(User user, string plainPassword) => hasher.HashPassword(user, plainPassword);

        var users = new List<User>();

        var admin = new User
        {
            Username = "admin",
            FullName = "Thủ thư",
            Email = "admin@dainam.edu.vn",
            PhoneNumber = "0900000000",
            Role = 1,
            IsActive = true,
            TotalFine = 0,
            CreatedAt = DateTime.Now,
            StudentCode = null
        };
        admin.PasswordHash = Hash(admin, "admin123");
        users.Add(admin);

        for (int i = 1; i <= 30; i++)
        {
            var code = $"SV{i:0000}";
            var student = new User
            {
                StudentCode = code,
                Username = $"sv{i:0000}",
                FullName = $"Sinh viên {i:00}",
                Email = $"sv{i:0000}@dainam.edu.vn",
                PhoneNumber = $"090{i:0000000}",     // unique
                Role = 2,
                IsActive = true,
                TotalFine = 0,
                CreatedAt = DateTime.Now.AddDays(-rnd.Next(1, 120))
            };
            student.PasswordHash = Hash(student, "123456Aa!");
            users.Add(student);
        }

        db.Users.AddRange(users);

        // =====================
        // 3) BOOKS (50)
        // 10 sách mỗi thể loại, mã TL-NNNN
        // =====================
        var authors = new[] { "Nguyễn Văn A", "Trần Văn B", "Lê Thị C", "Phạm Văn D", "Hoàng E" };
        var publishers = new[] { "NXB Trẻ", "NXB Giáo Dục", "NXB Kim Đồng", "NXB Lao Động", "NXB Tổng Hợp" };

        var books = new List<Book>();
        foreach (var c in categories)
        {
            for (int i = 1; i <= 10; i++)
            {
                var bookId = $"{c.CategoryId}-{i:0000}";
                books.Add(new Book
                {
                    BookId = bookId,
                    Title = $"Sách {c.CategoryId} #{i:00}",
                    Author = authors[rnd.Next(authors.Length)],
                    Publisher = publishers[rnd.Next(publishers.Length)],
                    CategoryId = c.CategoryId,
                    PublishYear = rnd.Next(2010, 2025),
                    Quantity = rnd.Next(2, 8), // tồn kho ban đầu
                    Location = $"Kệ {c.CategoryId}-{rnd.Next(1, 6)}",
                    ImagePath = null
                });
            }
        }
        db.Books.AddRange(books);

        await db.SaveChangesAsync();

        // =====================
        // 4) LOANS (100)
        // 70 đã trả, 25 đang mượn, 5 quá hạn
        // =====================
        var readerIds = users.Where(u => u.Role == 2).Select(u => u.Id).ToList();
        var bookIds = books.Select(b => b.BookId).ToList();

        var loans = new List<Loan>();

        // Helper: chọn user còn <3 khoản đang mượn/ quá hạn
        int PickUserWithLessThan3Open()
        {
            while (true)
            {
                var uid = readerIds[rnd.Next(readerIds.Count)];
                var openCount = loans.Count(l => l.UserId == uid && (l.Status == LoanStatus.DangMuon || l.Status == LoanStatus.QuaHan));
                if (openCount < 3) return uid;
            }
        }

        // Helper: chọn sách còn quantity > 0 (để tạo phiếu đang mượn)
        string PickAvailableBook()
        {
            while (true)
            {
                var bid = bookIds[rnd.Next(bookIds.Count)];
                var b = books.First(x => x.BookId == bid);
                if (b.Quantity > 0) return bid;
            }
        }

        // 70 đã trả
        for (int i = 0; i < 70; i++)
        {
            var uid = readerIds[rnd.Next(readerIds.Count)];
            var bid = bookIds[rnd.Next(bookIds.Count)];

            var borrow = DateTime.Today.AddDays(-rnd.Next(5, 150));
            var due = borrow.AddDays(14);

            // trả trong khoảng borrow+1 đến due+10
            var returnDate = borrow.AddDays(rnd.Next(1, 25));
            if (returnDate < borrow) returnDate = borrow;

            var lateDays = (returnDate.Date - due.Date).Days;
            var fine = lateDays > 0 ? lateDays * 5000m : 0m;

            loans.Add(new Loan
            {
                UserId = uid,
                BookId = bid,
                BorrowDate = borrow,
                DueDate = due,
                ReturnDate = returnDate,
                Fine = fine,
                Status = LoanStatus.DaTra
            });
        }

        // 25 đang mượn (chưa quá hạn)
        for (int i = 0; i < 25; i++)
        {
            var uid = PickUserWithLessThan3Open();
            var bid = PickAvailableBook();

            var borrow = DateTime.Today.AddDays(-rnd.Next(0, 10));
            var due = borrow.AddDays(14);

            loans.Add(new Loan
            {
                UserId = uid,
                BookId = bid,
                BorrowDate = borrow,
                DueDate = due,
                ReturnDate = null,
                Fine = 0,
                Status = LoanStatus.DangMuon
            });

            // đang mượn => giảm tồn kho
            books.First(b => b.BookId == bid).Quantity -= 1;
        }

        // 5 quá hạn (chưa trả)
        for (int i = 0; i < 5; i++)
        {
            var uid = PickUserWithLessThan3Open();
            var bid = PickAvailableBook();

            var borrow = DateTime.Today.AddDays(-rnd.Next(20, 40));
            var due = borrow.AddDays(14); // chắc chắn < hôm nay

            loans.Add(new Loan
            {
                UserId = uid,
                BookId = bid,
                BorrowDate = borrow,
                DueDate = due,
                ReturnDate = null,
                Fine = 0, // có thể tính động khi hiển thị
                Status = LoanStatus.QuaHan
            });

            books.First(b => b.BookId == bid).Quantity -= 1;
        }

        db.Loans.AddRange(loans);

        // Cập nhật TotalFine demo: cộng các khoản fine của phiếu đã trả
        var fineByUser = loans
            .Where(l => l.Fine > 0)
            .GroupBy(l => l.UserId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Fine));

        foreach (var u in users.Where(x => x.Role == 2))
            if (fineByUser.TryGetValue(u.Id, out var total))
                u.TotalFine = total;

        await db.SaveChangesAsync();
    }
}