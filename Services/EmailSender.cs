using System.Net;
using System.Net.Mail;

namespace QuanLyThuVienTruongHoc.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp, string userName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var appPassword = _configuration["EmailSettings:AppPassword"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, appPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = "Mã OTP Đặt Lại Mật Khẩu - Thư Viện Đại Nam",
                    Body = GetEmailTemplate(otp, userName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation($"OTP email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
                throw new Exception("Không thể gửi email. Vui lòng thử lại sau.");
            }
        }

        private string GetEmailTemplate(string otp, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #003366, #0066cc); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 40px 30px; }}
        .otp-box {{ background-color: #f8f9fa; border: 2px dashed #f58220; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; color: #f58220; letter-spacing: 8px; margin: 10px 0; }}
        .info {{ color: #666; font-size: 14px; line-height: 1.6; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #888; font-size: 12px; border-radius: 0 0 10px 10px; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 ĐẶT LẠI MẬT KHẨU</h1>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>Thư Viện Đại Nam</p>
        </div>
        <div class='content'>
            <p class='info'>Xin chào <strong>{userName}</strong>,</p>
            <p class='info'>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình. Vui lòng sử dụng mã OTP dưới đây để tiếp tục:</p>
            
            <div class='otp-box'>
                <p style='margin: 0; color: #666; font-size: 14px;'>MÃ OTP CỦA BẠN</p>
                <div class='otp-code'>{otp}</div>
                <p style='margin: 10px 0 0 0; color: #999; font-size: 12px;'>Mã có hiệu lực trong 5 phút</p>
            </div>

            <div class='warning'>
                <strong>⚠️ Lưu ý:</strong>
                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                    <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                    <li>Mã sẽ hết hạn sau 5 phút kể từ khi nhận email này</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>

            <p class='info' style='margin-top: 30px;'>Nếu bạn gặp vấn đề, vui lòng liên hệ bộ phận hỗ trợ.</p>
        </div>
        <div class='footer'>
            <p>© 2026 Thư Viện Đại Nam. Hệ thống quản lý thư viện trường học.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
