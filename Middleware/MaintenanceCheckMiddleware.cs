using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using QuanLyThuVienTruongHoc.Services;
using System.Security.Claims;

namespace QuanLyThuVienTruongHoc.Middleware
{
    public class MaintenanceCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SystemSettingsService settingsService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 1. Luôn cho phép các trang sau:
            if (path.StartsWith("/account/logout") || 
                path.StartsWith("/account/logout") || 
                path.StartsWith("/account/maintenance") ||
                path.StartsWith("/account/maintenancelogin") ||
                path.StartsWith("/maintenance") ||
                path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                path.StartsWith("/lib/") || 
                path.StartsWith("/images/"))
            {
                await _next(context);
                return;
            }

            // 2. Lấy trạng thái bảo trì
            // Lưu ý: Cần inject SystemSettingsService vào InvokeAsync (Scoped service injection in Middleware)
            // Lấy trực tiếp settings
            var maintenanceMode = false;
            try 
            {
                var settings = await settingsService.GetSettingsAsync();
                maintenanceMode = settings.MaintenanceMode;
            }
            catch
            {
                // Fallback nếu lỗi DB
            }

            if (maintenanceMode)
            {
                // 3. Nếu đang bảo trì:
                // - Admin được phép truy cập tất cả
                // - User thường hoặc chưa đăng nhập -> Chặn

                // Kiểm tra user có phải Admin không
                // Lưu ý: Middleware này phải đặt SAU UseAuthentication để context.User có dữ liệu
                if (context.User.Identity != null && 
                    context.User.Identity.IsAuthenticated && 
                    context.User.IsInRole("Admin"))
                {
                    await _next(context);
                    return;
                }

                // Nếu là Admin Page login thì cho qua (đã xử lý ở trên /account/login)
                
                // Redirect về trang bảo trì
                context.Response.Redirect("/Maintenance");
                return;
            }

            await _next(context);
        }
    }
}
