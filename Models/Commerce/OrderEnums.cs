namespace QuanLyThuVienTruongHoc.Models.Commerce
{
    public enum PaymentMethod
    {
        Cod = 1,
        BankTransfer = 2
    }

    public enum PaymentStatus
    {
        Unpaid = 1,
        PendingConfirmation = 2,
        Paid = 3,
        Refunded = 4
    }

    public enum OrderStatus
    {
        Pending = 1,          // vừa tạo
        Confirmed = 2,        // admin xác nhận (hoặc auto với COD)
        Processing = 3,       // đóng gói
        Shipping = 4,         // đang giao
        Delivered = 5,        // đã giao
        Cancelled = 6         // hủy
    }
}

