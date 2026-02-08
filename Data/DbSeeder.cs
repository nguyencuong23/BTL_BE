using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Models.Library;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Đảm bảo đã migrate (COMMENT khi dùng EnsureCreated)
        // await db.Database.MigrateAsync();

        // Đã có dữ liệu thì không seed nữa
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
            FullName = "Nguyễn Mạnh Cường",
            // Email = "admin@dainam.edu.vn",
            Email = "kct2378@gmail.com",
            PhoneNumber = "0900000000",
            Role = 1,
            IsActive = true,
            TotalFine = 0,
            CreatedAt = DateTime.Now,
            StudentCode = null
        };
        admin.PasswordHash = Hash(admin, "admin123");
        users.Add(admin);

        var admin2 = new User
        {
            Username = "admin2",
            FullName = "Thủ thư 2",
            Email = "admin2@dainam.edu.vn",
            PhoneNumber = "0910000000",
            Role = 1,
            IsActive = true,
            TotalFine = 0,
            CreatedAt = DateTime.Now,
            StudentCode = null
        };
        admin2.PasswordHash = Hash(admin, "admin123");
        users.Add(admin2);

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
                CreatedAt = DateTime.Now
            };
            student.PasswordHash = Hash(student, "123456Aa!");
            users.Add(student);
        }

        db.Users.AddRange(users);

        // =====================
        // 3) BOOKS (50)
        // 10 sách mỗi thể loại, mã TL-NNNN
        // =====================
        var books = new List<Book>
        {
            // ================= CNTT (IT) =================
            new() { BookId="IT-0001", Title="Clean Code", Author="Robert C. Martin", Publisher="Prentice Hall", CategoryId="IT", PublishYear=2008, Quantity=25, Location="Kệ IT-1", ImagePath="/images/books/IT/IT-0001.jpg" },
            new() { BookId="IT-0002", Title="Clean Architecture", Author="Robert C. Martin", Publisher="Prentice Hall", CategoryId="IT", PublishYear=2017, Quantity=22, Location="Kệ IT-1", ImagePath="/images/books/IT/IT-0002.jpg" },
            new() { BookId="IT-0003", Title="Design Patterns", Author="Erich Gamma et al.", Publisher="Addison-Wesley", CategoryId="IT", PublishYear=1994, Quantity=20, Location="Kệ IT-1", ImagePath="/images/books/IT/IT-0003.jpg" },
            new() { BookId="IT-0004", Title="Refactoring", Author="Martin Fowler", Publisher="Addison-Wesley", CategoryId="IT", PublishYear=2018, Quantity=24, Location="Kệ IT-2", ImagePath="/images/books/IT/IT-0004.jpg" },
            new() { BookId="IT-0005", Title="Introduction to Algorithms", Author="Thomas H. Cormen", Publisher="MIT Press", CategoryId="IT", PublishYear=2009, Quantity=30, Location="Kệ IT-2", ImagePath="/images/books/IT/IT-0005.jpg" },
            new() { BookId="IT-0006", Title="The Pragmatic Programmer", Author="Andrew Hunt", Publisher="Addison-Wesley", CategoryId="IT", PublishYear=1999, Quantity=23, Location="Kệ IT-2", ImagePath="/images/books/IT/IT-0006.jpg" },
            new() { BookId="IT-0007", Title="Head First Design Patterns", Author="Eric Freeman", Publisher="O'Reilly", CategoryId="IT", PublishYear=2020, Quantity=28, Location="Kệ IT-3", ImagePath="/images/books/IT/IT-0007.jpg" },
            new() { BookId="IT-0008", Title="Artificial Intelligence: A Modern Approach", Author="Stuart Russell", Publisher="Pearson", CategoryId="IT", PublishYear=2021, Quantity=21, Location="Kệ IT-3", ImagePath="/images/books/IT/IT-0008.jpg" },
            new() { BookId="IT-0009", Title="Database System Concepts", Author="Silberschatz", Publisher="McGraw-Hill", CategoryId="IT", PublishYear=2019, Quantity=26, Location="Kệ IT-3", ImagePath="/images/books/IT/IT-0009.jpg" },
            new() { BookId="IT-0010", Title="Computer Networks", Author="Andrew S. Tanenbaum", Publisher="Pearson", CategoryId="IT", PublishYear=2010, Quantity=22, Location="Kệ IT-3", ImagePath="/images/books/IT/IT-0010.jpg" },

            // ================= VĂN HỌC (VH) =================
            new() { BookId="VH-0001", Title="Trăm năm cô đơn", Author="Gabriel García Márquez", Publisher="NXB Văn Học", CategoryId="VH", PublishYear=1967, Quantity=30, Location="Kệ VH-1", ImagePath="/images/books/VH/VH-0001.jpg" },
            new() { BookId="VH-0002", Title="Ông già và biển cả", Author="Ernest Hemingway", Publisher="NXB Văn Học", CategoryId="VH", PublishYear=1952, Quantity=28, Location="Kệ VH-1", ImagePath="/images/books/VH/VH-0002.jpg" },
            new() { BookId="VH-0003", Title="Nhà giả kim", Author="Paulo Coelho", Publisher="NXB Lao Động", CategoryId="VH", PublishYear=1988, Quantity=25, Location="Kệ VH-1", ImagePath="/images/books/VH/VH-0003.jpg" },
            new() { BookId="VH-0004", Title="1984", Author="George Orwell", Publisher="Secker & Warburg", CategoryId="VH", PublishYear=1949, Quantity=20, Location="Kệ VH-2", ImagePath="/images/books/VH/VH-0004.jpg" },
            new() { BookId="VH-0005", Title="Bố già", Author="Mario Puzo", Publisher="G. P. Putnam's Sons", CategoryId="VH", PublishYear=1969, Quantity=22, Location="Kệ VH-2", ImagePath="/images/books/VH/VH-0005.jpg" },
            new() { BookId="VH-0006", Title="Tội ác và hình phạt", Author="Fyodor Dostoevsky", Publisher="NXB Văn Học", CategoryId="VH", PublishYear=1866, Quantity=24, Location="Kệ VH-2", ImagePath="/images/books/VH/VH-0006.jpg" },
            new() { BookId="VH-0007", Title="Giết con chim nhại", Author="Harper Lee", Publisher="J. B. Lippincott", CategoryId="VH", PublishYear=1960, Quantity=21, Location="Kệ VH-3", ImagePath="/images/books/VH/VH-0007.jpg" },
            new() { BookId="VH-0008", Title="Cuốn theo chiều gió", Author="Margaret Mitchell", Publisher="Macmillan", CategoryId="VH", PublishYear=1936, Quantity=23, Location="Kệ VH-3", ImagePath="/images/books/VH/VH-0008.jpg" },
            new() { BookId="VH-0009", Title="Hoàng tử bé", Author="Antoine de Saint-Exupéry", Publisher="Reynal & Hitchcock", CategoryId="VH", PublishYear=1943, Quantity=27, Location="Kệ VH-3", ImagePath="/images/books/VH/VH-0009.jpg" },
            new() { BookId="VH-0010", Title="Những người khốn khổ", Author="Victor Hugo", Publisher="A. Lacroix", CategoryId="VH", PublishYear=1862, Quantity=25, Location="Kệ VH-3", ImagePath="/images/books/VH/VH-0010.jpg" },

            // ================= KINH TẾ (KT) =================
            new() { BookId="KT-0001", Title="Cha giàu cha nghèo", Author="Robert Kiyosaki", Publisher="NXB Trẻ", CategoryId="KT", PublishYear=1997, Quantity=30, Location="Kệ KT-1", ImagePath="/images/books/KT/KT-0001.jpg" },
            new() { BookId="KT-0002", Title="Tư duy nhanh và chậm", Author="Daniel Kahneman", Publisher="NXB Lao Động", CategoryId="KT", PublishYear=2011, Quantity=24, Location="Kệ KT-1", ImagePath="/images/books/KT/KT-0002.jpg" },
            new() { BookId="KT-0003", Title="The Lean Startup", Author="Eric Ries", Publisher="Crown Publishing", CategoryId="KT", PublishYear=2011, Quantity=22, Location="Kệ KT-1", ImagePath="/images/books/KT/KT-0003.jpg" },
            new() { BookId="KT-0004", Title="Zero to One", Author="Peter Thiel", Publisher="Crown Business", CategoryId="KT", PublishYear=2014, Quantity=21, Location="Kệ KT-2", ImagePath="/images/books/KT/KT-0004.jpg" },
            new() { BookId="KT-0005", Title="The Intelligent Investor", Author="Benjamin Graham", Publisher="HarperBusiness", CategoryId="KT", PublishYear=1949, Quantity=25, Location="Kệ KT-2", ImagePath="/images/books/KT/KT-0005.jpg" },
            new() { BookId="KT-0006", Title="Principles", Author="Ray Dalio", Publisher="Simon & Schuster", CategoryId="KT", PublishYear=2017, Quantity=20, Location="Kệ KT-2", ImagePath="/images/books/KT/KT-0006.jpg" },
            new() { BookId="KT-0007", Title="Marketing 4.0", Author="Philip Kotler", Publisher="Wiley", CategoryId="KT", PublishYear=2017, Quantity=23, Location="Kệ KT-3", ImagePath="/images/books/KT/KT-0007.jpg" },
            new() { BookId="KT-0008", Title="Blue Ocean Strategy", Author="W. Chan Kim", Publisher="Harvard Business", CategoryId="KT", PublishYear=2005, Quantity=22, Location="Kệ KT-3", ImagePath="/images/books/KT/KT-0008.jpg" },
            new() { BookId="KT-0009", Title="Start With Why", Author="Simon Sinek", Publisher="Portfolio", CategoryId="KT", PublishYear=2009, Quantity=21, Location="Kệ KT-3", ImagePath="/images/books/KT/KT-0009.jpg" },
            new() { BookId="KT-0010", Title="Good to Great", Author="Jim Collins", Publisher="HarperBusiness", CategoryId="KT", PublishYear=2001, Quantity=24, Location="Kệ KT-3", ImagePath="/images/books/KT/KT-0010.jpg" },

            // ================= KHOA HỌC (KH) =================
            new() { BookId="KH-0001", Title="Lược sử thời gian", Author="Stephen Hawking", Publisher="Bantam", CategoryId="KH", PublishYear=1988, Quantity=30, Location="Kệ KH-1", ImagePath="/images/books/KH/KH-0001.jpg" },
            new() { BookId="KH-0002", Title="Vũ trụ trong vỏ hạt dẻ", Author="Stephen Hawking", Publisher="Bantam", CategoryId="KH", PublishYear=2001, Quantity=25, Location="Kệ KH-1", ImagePath="/images/books/KH/KH-0002.jpg" },
            new() { BookId="KH-0003", Title="Sapiens", Author="Yuval Noah Harari", Publisher="NXB Tri Thức", CategoryId="KH", PublishYear=2011, Quantity=28, Location="Kệ KH-1", ImagePath="/images/books/KH/KH-0003.jpg" },
            new() { BookId="KH-0004", Title="Homo Deus", Author="Yuval Noah Harari", Publisher="NXB Tri Thức", CategoryId="KH", PublishYear=2015, Quantity=24, Location="Kệ KH-2", ImagePath="/images/books/KH/KH-0004.jpg" },
            new() { BookId="KH-0005", Title="The Selfish Gene", Author="Richard Dawkins", Publisher="Oxford", CategoryId="KH", PublishYear=1976, Quantity=22, Location="Kệ KH-2", ImagePath="/images/books/KH/KH-0005.jpg" },
            new() { BookId="KH-0006", Title="A Brief History of Nearly Everything", Author="Bill Bryson", Publisher="Broadway", CategoryId="KH", PublishYear=2003, Quantity=20, Location="Kệ KH-2", ImagePath="/images/books/KH/KH-0006.jpg" },
            new() { BookId="KH-0007", Title="Cosmos", Author="Carl Sagan", Publisher="Random House", CategoryId="KH", PublishYear=1980, Quantity=23, Location="Kệ KH-3", ImagePath="/images/books/KH/KH-0007.jpg" },
            new() { BookId="KH-0008", Title="The Elegant Universe", Author="Brian Greene", Publisher="W. W. Norton", CategoryId="KH", PublishYear=1999, Quantity=21, Location="Kệ KH-3", ImagePath="/images/books/KH/KH-0008.jpg" },
            new() { BookId="KH-0009", Title="The Origin of Species", Author="Charles Darwin", Publisher="John Murray", CategoryId="KH", PublishYear=1859, Quantity=26, Location="Kệ KH-3", ImagePath="/images/books/KH/KH-0009.jpg" },
            new() { BookId="KH-0010", Title="Thinking, Fast and Slow (Science)", Author="Daniel Kahneman", Publisher="Farrar", CategoryId="KH", PublishYear=2011, Quantity=20, Location="Kệ KH-3", ImagePath="/images/books/KH/KH-0010.jpg" },

            // ================= NGOẠI NGỮ (NN) =================
            new() { BookId="NN-0001", Title="English Grammar in Use", Author="Raymond Murphy", Publisher="Cambridge", CategoryId="NN", PublishYear=2019, Quantity=30, Location="Kệ NN-1", ImagePath="/images/books/NN/NN-0001.jpg" },
            new() { BookId="NN-0002", Title="Oxford Advanced Learner's Dictionary", Author="Oxford", Publisher="Oxford", CategoryId="NN", PublishYear=2020, Quantity=25, Location="Kệ NN-1", ImagePath="/images/books/NN/NN-0002.jpg" },
            new() { BookId="NN-0003", Title="Cambridge IELTS 16", Author="Cambridge", Publisher="Cambridge", CategoryId="NN", PublishYear=2021, Quantity=28, Location="Kệ NN-1", ImagePath="/images/books/NN/NN-0003.jpg" },
            new() { BookId="NN-0004", Title="Minna no Nihongo I", Author="3A Corporation", Publisher="3A", CategoryId="NN", PublishYear=2018, Quantity=24, Location="Kệ NN-2", ImagePath="/images/books/NN/NN-0004.jpg" },
            new() { BookId="NN-0005", Title="Minna no Nihongo II", Author="3A Corporation", Publisher="3A", CategoryId="NN", PublishYear=2018, Quantity=23, Location="Kệ NN-2", ImagePath="/images/books/NN/NN-0005.jpg" },
            new() { BookId="NN-0006", Title="Genki I", Author="Eri Banno", Publisher="The Japan Times", CategoryId="NN", PublishYear=2020, Quantity=22, Location="Kệ NN-2", ImagePath="/images/books/NN/NN-0006.jpg" },
            new() { BookId="NN-0007", Title="Genki II", Author="Eri Banno", Publisher="The Japan Times", CategoryId="NN", PublishYear=2020, Quantity=21, Location="Kệ NN-3", ImagePath="/images/books/NN/NN-0007.jpg" },
            new() { BookId="NN-0008", Title="New Headway", Author="Liz & John Soars", Publisher="Oxford", CategoryId="NN", PublishYear=2015, Quantity=20, Location="Kệ NN-3", ImagePath="/images/books/NN/NN-0008.jpg" },
            new() { BookId="NN-0009", Title="English Vocabulary in Use", Author="Michael McCarthy", Publisher="Cambridge", CategoryId="NN", PublishYear=2017, Quantity=26, Location="Kệ NN-3", ImagePath="/images/books/NN/NN-0009.jpg" },
            new() { BookId="NN-0010", Title="Japanese for Busy People", Author="AJALT", Publisher="Kodansha", CategoryId="NN", PublishYear=2018, Quantity=24, Location="Kệ NN-3", ImagePath="/images/books/NN/NN-0010.jpg" }
        };

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

            // Tính phạt nóng cho sách quá hạn
            var overdueDays = (DateTime.Today - due).Days;
            var fine = overdueDays > 0 ? overdueDays * 5000m : 0m;

            loans.Add(new Loan
            {
                UserId = uid,
                BookId = bid,
                BorrowDate = borrow,
                DueDate = due,
                ReturnDate = null,
                Fine = fine, 
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
        {
            if (fineByUser.TryGetValue(u.Id, out var total))
            {
                u.TotalFine = total;
            }
            // Khóa tài khoản nếu nợ > 50,000
            if (u.TotalFine > 50000)
            {
                u.IsActive = false;
            }
        }

        await db.SaveChangesAsync();
    }
}