using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyThuVienTruongHoc.Controllers
{
    /// <summary>
    /// API quản lý Thông tin cá nhân người dùng đang đăng nhập
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin user đang đăng nhập
        /// GET: api/profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == int.Parse(userId))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân (KHÔNG cho sửa Role)
        /// PUT: api/profile
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.FullName = model.FullName;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thông tin thành công");
        }
    }
}
