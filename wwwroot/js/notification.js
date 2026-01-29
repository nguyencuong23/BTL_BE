document.addEventListener("DOMContentLoaded", function () {
  // --- 1. LOGIC TÔ MÀU TRANG ĐANG TRUY CẬP (ACTIVE LINK) ---
  function handleActiveNavbar() {
    const currentPath = window.location.pathname.toLowerCase();
    // Lấy tất cả link trong nav (loại trừ các nút dropdown và item trong dropdown nếu muốn)
    const navLinks = document.querySelectorAll("nav a");

    navLinks.forEach((link) => {
      const href = link.getAttribute("href");
      if (!href || href === "#") return;

      const linkPath = href.toLowerCase();

      // Kiểm tra khớp đường dẫn
      const isHome =
        (currentPath === "/" ||
          currentPath === "/client" ||
          currentPath === "/client/index") &&
        (linkPath === "/" || linkPath.includes("index"));

      const isExactMatch = currentPath === linkPath;
      const isSubPage = currentPath.includes(linkPath) && linkPath.length > 5;

      if (isHome || isExactMatch || isSubPage) {
        link.classList.add("nav-active");
      } else {
        link.classList.remove("nav-active");
      }
    });
  }

  // --- 2. LOGIC MODAL THÔNG BÁO ---
  function createPremiumModal() {
    // Kiểm tra nếu modal đã tồn tại thì không tạo thêm
    if (document.getElementById("libPremiumModal")) return;

    const modalHTML = `
        <div class="modal fade modal-vivid" id="libPremiumModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered modal-lg">
                <div class="modal-content shadow-lg">
                    <div class="modal-header-premium text-center">
                        <i class="fa-solid fa-circle-info mb-3" style="font-size: 3rem; color: #f59e0b;"></i>
                        <h2 class="fw-bold mb-1">BẢN TIN THƯ VIỆN</h2>
                        <p class="opacity-75">Cập nhật những thông tin mới nhất hôm nay</p>
                    </div>
                    <div class="modal-body p-4">
                        <div class="notif-card">
                            <div class="notif-icon-circle bg-primary"><i class="fa-solid fa-book-open"></i></div>
                            <div>
                                <h6 class="fw-bold mb-1">Sách mới về kho!</h6>
                                <p class="small text-muted mb-0">Hơn 200 cuốn giáo trình CNTT vừa cập nhật tại tầng 2.</p>
                            </div>
                        </div>
                        <div class="notif-card">
                            <div class="notif-icon-circle bg-warning"><i class="fa-solid fa-star"></i></div>
                            <div>
                                <h6 class="fw-bold mb-1">Sự kiện: Ngày hội đọc sách</h6>
                                <p class="small text-muted mb-0">Tham gia nhận quà vào sáng Thứ 7 này tại sảnh chính.</p>
                            </div>
                        </div>
                        <div class="notif-card">
                            <div class="notif-icon-circle bg-success"><i class="fa-solid fa-wifi"></i></div>
                            <div>
                                <h6 class="fw-bold mb-1">Nâng cấp Wifi Thư viện</h6>
                                <p class="small text-muted mb-0">Hệ thống mạng đã được nâng cấp lên tốc độ cao.</p>
                            </div>
                        </div>
                        <button type="button" class="btn-premium w-100 mt-3" data-bs-dismiss="modal">
                            TÔI ĐÃ NẮM ĐƯỢC <i class="fa-solid fa-arrow-right ms-2"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>`;
    document.body.insertAdjacentHTML("beforeend", modalHTML);
  }

  // --- 3. KÍCH HOẠT CÁC CHỨC NĂNG ---
  handleActiveNavbar(); // Chạy ngay khi load trang
  createPremiumModal();

  const bellBtn = document.getElementById("bellBtn");
  const modalElement = document.getElementById("libPremiumModal");

  if (bellBtn && modalElement) {
    const myModal = new bootstrap.Modal(modalElement);
    bellBtn.onclick = function () {
      myModal.show();
      const ping = document.getElementById("badgeCount");
      if (ping) ping.style.display = "none";
    };
  }
});
