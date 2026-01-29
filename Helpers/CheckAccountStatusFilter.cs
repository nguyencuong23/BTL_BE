using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using QuanLyThuVienTruongHoc.Data;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyThuVienTruongHoc.Helpers
{
    public class CheckAccountStatusFilter : IAsyncActionFilter
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CheckAccountStatusFilter(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // Nếu user đã đăng nhập
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Tạo scope mới để lấy DbContext (vì Filter có thể là Singleton hoặc Scoped, nhưng DbContext là Scoped)
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var dbUser = await dbContext.Users.FindAsync(userId);

                        // Nếu user bị khóa (IsActive = false)
                        if (dbUser != null && !dbUser.IsActive)
                        {
                            // Lấy thông tin Controller/Action hiện tại
                            var controller = context.RouteData.Values["controller"]?.ToString();
                            var action = context.RouteData.Values["action"]?.ToString();

                            // Cho phép truy cập trang Logout, Login và Maintenance
                            bool isAllowed = (controller == "Account" && (action == "Logout" || action == "Login" || action == "Maintenance"));

                            if (!isAllowed)
                            {
                                // Force logout
                                await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext, 
                                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                                context.Result = new RedirectToActionResult("Login", "Account", new { locked = true });
                                return;
                            }
                        }
                    }
                }
            }

            await next();
        }
    }
}
