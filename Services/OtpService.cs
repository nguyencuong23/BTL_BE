using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVienTruongHoc.Data;
using QuanLyThuVienTruongHoc.Models.Users;

namespace QuanLyThuVienTruongHoc.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OtpService> _logger;
        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPT_COUNT = 3;

        public OtpService(ApplicationDbContext context, ILogger<OtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateOtpAsync(string email)
        {
            // Invalidate tất cả OTP cũ của email này
            var oldOtps = await _context.PasswordResetOtps
                .Where(p => p.Email == email && !p.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            // Tạo OTP mới
            var otp = GenerateOtp();
            var hasher = new PasswordHasher<PasswordResetOtp>();
            
            var otpEntity = new PasswordResetOtp
            {
                Email = email,
                OtpHash = "", // Temporary, sẽ được set bên dưới
                ExpireAt = DateTime.Now.AddMinutes(OTP_EXPIRY_MINUTES),
                AttemptCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            // Hash OTP
            otpEntity.OtpHash = hasher.HashPassword(otpEntity, otp);

            _context.PasswordResetOtps.Add(otpEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"OTP created for email: {email}");

            return otp;
        }

        public async Task<(bool isValid, string? errorMessage)> ValidateOtpAsync(string email, string otp)
        {
            var otpRecord = await _context.PasswordResetOtps
                .Where(p => p.Email == email && !p.IsUsed)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return (false, "OTP không đúng hoặc đã hết hạn");
            }

            // Check expired
            if (DateTime.Now > otpRecord.ExpireAt)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                return (false, "OTP không đúng hoặc đã hết hạn");
            }

            // Check attempt count
            if (otpRecord.AttemptCount >= MAX_ATTEMPT_COUNT)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                return (false, "Bạn đã nhập sai OTP quá 3 lần. Vui lòng yêu cầu OTP mới");
            }

            // Verify OTP
            var hasher = new PasswordHasher<PasswordResetOtp>();
            var result = hasher.VerifyHashedPassword(otpRecord, otpRecord.OtpHash, otp);

            if (result == PasswordVerificationResult.Failed)
            {
                otpRecord.AttemptCount++;
                await _context.SaveChangesAsync();

                var remainingAttempts = MAX_ATTEMPT_COUNT - otpRecord.AttemptCount;
                if (remainingAttempts > 0)
                {
                    return (false, $"OTP không đúng. Bạn còn {remainingAttempts} lần thử");
                }
                else
                {
                    otpRecord.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return (false, "OTP không đúng hoặc đã hết hạn");
                }
            }

            _logger.LogInformation($"OTP validated successfully for email: {email}");
            return (true, null);
        }

        public async Task MarkOtpAsUsedAsync(string email)
        {
            var otpRecord = await _context.PasswordResetOtps
                .Where(p => p.Email == email && !p.IsUsed)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord != null)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"OTP marked as used for email: {email}");
            }
        }

        private string GenerateOtp()
        {
            var random = new Random();
            var otp = "";
            for (int i = 0; i < OTP_LENGTH; i++)
            {
                otp += random.Next(0, 10).ToString();
            }
            return otp;
        }
    }
}
