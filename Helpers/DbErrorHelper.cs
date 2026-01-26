using System;

namespace QuanLyThuVienTruongHoc.Helpers
{
    public static class DbErrorHelper
    {
        public static string TranslateDbError(Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;

            // Duplicate key errors
            if (message.Contains("duplicate key") || message.Contains("Cannot insert duplicate key"))
            {
                if (message.Contains("IX_Users_Username") || message.Contains("'Username'"))
                {
                    return "Tên đăng nhập đã tồn tại. Vui lòng chọn tên đăng nhập khác.";
                }
                if (message.Contains("IX_Users_StudentCode") || message.Contains("'StudentCode'"))
                {
                    return "Mã sinh viên đã tồn tại. Vui lòng nhập mã sinh viên khác.";
                }
                if (message.Contains("IX_Categories_CategoryId") || message.Contains("'CategoryId'"))
                {
                    return "Mã thể loại đã tồn tại. Vui lòng chọn mã khác.";
                }
                if (message.Contains("IX_Books_BookId") || message.Contains("'BookId'"))
                {
                    return "Mã sách đã tồn tại. Vui lòng chọn mã khác.";
                }
                return "Dữ liệu bị trùng lặp. Vui lòng kiểm tra lại thông tin.";
            }

            // Foreign key constraint errors
            if (message.Contains("FOREIGN KEY constraint") || message.Contains("FK_"))
            {
                if (message.Contains("DELETE"))
                {
                    return "Không thể xóa vì dữ liệu đang được sử dụng ở nơi khác trong hệ thống.";
                }
                return "Dữ liệu tham chiếu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            // Required field errors
            if (message.Contains("Cannot insert the value NULL") || message.Contains("NOT NULL"))
            {
                return "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường có dấu (*).";
            }

            // String length errors
            if (message.Contains("String or binary data would be truncated"))
            {
                return "Dữ liệu nhập vào quá dài. Vui lòng rút ngắn lại.";
            }

            // Range/value errors
            if (message.Contains("out of range") || message.Contains("value is out of range"))
            {
                return "Giá trị nhập vào không hợp lệ. Vui lòng kiểm tra lại số liệu.";
            }

            // CHECK constraint errors
            if (message.Contains("CHECK constraint"))
            {
                return "Giá trị nhập vào không nằm trong khoảng cho phép. Vui lòng kiểm tra lại.";
            }

            // Default fallback
            return "Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin đã nhập.";
        }
    }
}
