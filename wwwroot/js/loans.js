document.addEventListener('DOMContentLoaded', function () {
    // 1. TỰ ĐỘNG HIỆN MODAL KHI LOAD TRANG
    var modal = document.getElementById('instruction-modal');
    if (modal) {
        // Thêm delay nhỏ để modal hiện ra mượt hơn sau khi trang load
        setTimeout(() => {
            modal.classList.add('active');
        }, 500);
    }

    // Xử lý đóng modal khi click ra ngoài
    window.onclick = function (event) {
        if (event.target == modal) {
            closeInstructionModal();
        }
    }

    // 2. CHẠY HIỆU ỨNG SỐ NHẢY (COUNTER)
    animateCounters();
});

// Hàm đóng modal
function closeInstructionModal() {
    var modal = document.getElementById('instruction-modal');
    if (modal) {
        modal.classList.remove('active');
    }
}

// 3. Hàm hiệu ứng số nhảy (Counter Animation)
function animateCounters() {
    const counters = document.querySelectorAll('.glass-panel h2'); // Chọn các thẻ h2 chứa số
    const speed = 200; // Tốc độ chạy (càng thấp càng nhanh)

    counters.forEach(counter => {
        const updateCount = () => {
            const target = +counter.innerText; // Lấy số đích (từ HTML server render)
            const count = +counter.getAttribute('data-val') || 0; // Lấy số hiện tại

            // Tính bước nhảy
            const inc = target / speed;

            if (count < target) {
                // Làm tròn lên và gán giá trị tạm
                const newVal = Math.ceil(count + inc);
                counter.setAttribute('data-val', newVal);
                counter.innerText = newVal;

                // Gọi lại hàm đệ quy để chạy tiếp
                setTimeout(updateCount, 20);
            } else {
                counter.innerText = target; // Đảm bảo số cuối cùng chính xác
            }
        };

        // Reset về 0 trước khi chạy
        counter.setAttribute('data-val', 0);
        const originalValue = counter.innerText;
        counter.innerText = "0";

        // Khôi phục giá trị đích để script chạy
        setTimeout(() => {
            counter.innerText = originalValue;
            updateCount();
        }, 800); // Đợi các animation CSS chạy xong mới bắt đầu đếm số
    });
}

// 4. Xử lý Tìm kiếm Real-time (Filter)
function filterLoans() {
    var input = document.getElementById("loanSearchInput");
    var filter = input.value.toUpperCase();
    var tableBody = document.getElementById("loanTableBody");
    var tr = tableBody.getElementsByTagName("tr");
    var hasResult = false;

    // Reset delay animation khi tìm kiếm để tránh bị lag
    var delayCount = 0;

    for (var i = 0; i < tr.length; i++) {
        var titleEl = tr[i].querySelector(".book-title");
        var authorEl = tr[i].querySelector(".book-author");

        if (titleEl && authorEl) {
            var txtValue = titleEl.textContent + " " + authorEl.textContent;

            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";

                // Thêm lại animation fade-in nhẹ khi search
                tr[i].style.animation = "none";
                tr[i].offsetHeight; /* trigger reflow */
                tr[i].style.animation = `fadeInUp 0.3s ease forwards ${delayCount * 0.05}s`;
                delayCount++;

                hasResult = true;
            } else {
                tr[i].style.display = "none";
            }
        }
    }

    var noResultMsg = document.getElementById("noResultsMsg");
    if (noResultMsg) {
        noResultMsg.style.display = hasResult ? "none" : "block";
        if (!hasResult) {
            noResultMsg.style.animation = "fadeInUp 0.5s ease";
        }
    }
}

function clearSearch() {
    var input = document.getElementById("loanSearchInput");
    input.value = "";
    filterLoans();
    input.focus();
}