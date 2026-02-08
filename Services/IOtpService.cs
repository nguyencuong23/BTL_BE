namespace QuanLyThuVienTruongHoc.Services
{
    public interface IOtpService
    {
        /// <summary>
        /// Tạo OTP mới cho email và lưu vào database
        /// </summary>
        /// <param name="email">Email của user</param>
        /// <returns>OTP string (6 chữ số)</returns>
        Task<string> CreateOtpAsync(string email);

        /// <summary>
        /// Validate OTP
        /// </summary>
        /// <param name="email">Email của user</param>
        /// <param name="otp">OTP user nhập vào</param>
        /// <returns>(isValid, errorMessage)</returns>
        Task<(bool isValid, string? errorMessage)> ValidateOtpAsync(string email, string otp);

        /// <summary>
        /// Đánh dấu OTP đã được sử dụng sau khi reset password thành công
        /// </summary>
        Task MarkOtpAsUsedAsync(string email);
    }
}
