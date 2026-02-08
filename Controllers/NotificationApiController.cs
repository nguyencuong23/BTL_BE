using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyThuVienTruongHoc.Services;

namespace QuanLyThuVienTruongHoc.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationApiController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationApiController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int? GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var uid) ? uid : null;
        }

        /// <summary>
        /// GET /api/notifications - Lấy danh sách thông báo (và chạy kiểm tra tạo mới khi mở).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.CheckAndGenerateNotificationsAsync(userId.Value);
            var list = await _notificationService.GetNotificationsAsync(userId.Value);
            return Ok(list.Select(n => new
            {
                n.Id,
                Type = n.Type.ToString(),
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                n.CreatedAt,
                n.RelatedEntityId
            }));
        }

        /// <summary>
        /// GET /api/notifications/unread-count - Số thông báo chưa đọc (badge).
        /// Gọi kiểm tra tạo thông báo trước để badge hiển thị ngay khi load trang.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.CheckAndGenerateNotificationsAsync(userId.Value);
            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(new { count });
        }

        /// <summary>
        /// POST /api/notifications/{id}/read - Đánh dấu đã đọc một thông báo.
        /// </summary>
        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var ok = await _notificationService.MarkReadAsync(id, userId.Value);
            if (!ok) return NotFound();
            return Ok();
        }

        /// <summary>
        /// POST /api/notifications/read-all - Đánh dấu tất cả đã đọc.
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllReadAsync(userId.Value);
            return Ok();
        }
    }
}
