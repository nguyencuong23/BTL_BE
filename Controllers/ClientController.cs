using Microsoft.AspNetCore.Authorization; // 1. Bắt buộc phải có thư viện này
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVienTruongHoc.Models.Common;
using System.Diagnostics;

namespace QuanLyThuVienTruongHoc.Controllers
{
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }

        // Ai cũng xem được trang chủ
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // 👇 QUAN TRỌNG: Dòng này chặn người chưa đăng nhập
        [Authorize]
        public IActionResult TraCuu()
        {
            return View();
        }

        // Ai cũng xem được tin tức
        public IActionResult News()
        {
            return View();
        }
        public IActionResult Payback()
        {
            return View();
        }

        // Trang test chỉ dành cho user đã đăng nhập
        [Authorize]
        public IActionResult UserOnly()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}