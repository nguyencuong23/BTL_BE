// Hàm lật sách
function togglePage() {
    const book = document.getElementById('theBook');
    // Ta vẫn dùng class 'show-register' của CSS cũ để kích hoạt animation
    // dù nội dung bây giờ là form khôi phục mật khẩu.
    book.classList.toggle('show-register');
}

// Xử lý sự kiện gửi yêu cầu (Optional: Tạo hiệu ứng loading)
function handleRecovery(e) {
    // e.preventDefault(); // Bỏ comment dòng này nếu muốn test giao diện mà không submit form

    const btn = e.target.querySelector('button');
    const originalText = btn.innerHTML;

    // Hiệu ứng nút bấm đang xử lý
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> ĐANG GỬI...';
    btn.style.opacity = '0.7';

    // Form sẽ tự động submit lên server theo asp-action
    return true;
}