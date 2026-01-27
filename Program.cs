using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. [QUAN TRỌNG] Thêm CORS để tránh lỗi chặn API từ trình duyệt
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 3. Add Services


builder.Services.AddControllers(); // Dành cho API
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CheckAccountStatusFilter>();
}); // Dành cho MVC
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Persist DataProtection keys to file system to keep cookies valid across restarts
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("DataProtectionKeys"));

// 4. Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        // Lưu ý: Dòng này làm Logout mỗi khi Restart server. 
        // Nếu muốn giữ đăng nhập lâu dài thì nên bỏ "+ Guid.NewGuid()".
        // Đặt tên tĩnh để không bị mất phiên đăng nhập khi restart server
        options.Cookie.Name = "DaiNamLib_Auth";
    });

var app = builder.Build();

// 5. Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Client/Error");
    app.UseHsts();
}

// Bật Swagger ở chế độ Dev để test API dễ hơn
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// [QUAN TRỌNG] Kích hoạt CORS (Đặt giữa UseRouting và UseAuthentication)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// 6. Khởi tạo Database (Đã sửa lỗi xóa dữ liệu)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

// Seed dữ liệu mẫu (Admin, Sách...)
await DbSeeder.SeedAsync(app.Services);

// 7. Định tuyến (Routing)
// [QUAN TRỌNG] MapControllers cho API hoạt động
app.MapControllers();

// Map Controller mặc định cho MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Client}/{action=Index}/{id?}");

app.Run();