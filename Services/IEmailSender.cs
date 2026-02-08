namespace QuanLyThuVienTruongHoc.Services
{
    public interface IEmailSender
    {
        /// <summary>
        /// Gửi email chứa OTP để reset mật khẩu
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <param name="otp">Mã OTP 6 chữ số</param>
        /// <param name="userName">Tên người dùng</param>
        /// <returns></returns>
        Task SendOtpEmailAsync(string toEmail, string otp, string userName);
    }
}
