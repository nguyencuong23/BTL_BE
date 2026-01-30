document.addEventListener("DOMContentLoaded", function () {
    // 1. Khởi tạo hiệu ứng AOS (Animation)
    AOS.init({ offset: 100, duration: 800, easing: "ease-in-out", once: true });

    // 2. Khởi tạo Swiper (Slide sách)
    new Swiper(".myBookSwiper", {
        slidesPerView: 1,
        spaceBetween: 20,
        loop: true,
        grabCursor: true,
        autoplay: {
            delay: 2500,
            disableOnInteraction: false,
        },
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
            dynamicBullets: true,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        breakpoints: {
            640: { slidesPerView: 2, spaceBetween: 20 },
            768: { slidesPerView: 3, spaceBetween: 30 },
            1024: { slidesPerView: 4, spaceBetween: 30 },
        },
    });
});

// --- CÁC HÀM TÌM KIẾM ---
function performSearch() {
    const input = document.getElementById("homeSearchInput");
    const keyword = input.value.trim();

    if (keyword) {
        window.location.href = `/Client/Search?search=${encodeURIComponent(keyword)}`;
    } else {
        input.focus();
        input.style.border = "2px solid #ffc107";
        setTimeout(() => (input.style.border = "none"), 1000);
    }
}

function handleEnter(event) {
    if (event.key === "Enter") {
        performSearch();
    }
}

// --- DỮ LIỆU NỘI DUNG POPUP (Đã cập nhật phần categories) ---
const serviceData = {
    reading: {
        title: "Không gian Đọc & Tự học Hiện đại",
        image: "https://images.unsplash.com/photo-1568667256549-094345857637?auto=format&fit=crop&w=1920&q=80",
        content: `
            <p class="lead text-secondary">Thư viện Đại Nam cung cấp không gian yên tĩnh với 500 chỗ ngồi.</p>
            <hr class="my-5">
            <div class="row mt-5">
                <div class="col-md-6">
                    <h4 class="fw-bold mb-4" style="color: #2c3e50;"><i class="fas fa-star text-warning me-2"></i>Tiện ích nổi bật</h4>
                    <ul class="list-unstyled svc-list fa-ul">
                        <li><span class="fa-li"><i class="fas fa-wifi"></i></span>Wifi tốc độ cao</li>
                        <li><span class="fa-li"><i class="fas fa-plug"></i></span>Ổ cắm điện tại bàn</li>
                        <li><span class="fa-li"><i class="fas fa-chair"></i></span>Ghế Ergonomic</li>
                    </ul>
                </div>
                <div class="col-md-6">
                    <div class="svc-feature-box shadow-sm">
                        <h5 class="fw-bold"><i class="fas fa-info-circle me-2"></i>Quy định</h5>
                        <p class="mb-0">Vui lòng giữ trật tự và vệ sinh chung.</p>
                    </div>
                </div>
            </div>
        `,
    },
    meeting: {
        title: "Dịch vụ Phòng Họp Nhóm",
        image: "https://images.unsplash.com/photo-1557804506-669a67965ba0?auto=format&fit=crop&w=1920&q=80",
        content: `
            <p class="lead text-secondary">Không gian thảo luận nhóm cách âm tốt.</p>
            <hr class="my-5">
            <div class="row mt-5 align-items-center">
                <div class="col-md-6">
                    <img src="https://images.unsplash.com/photo-1542744094-3a31f272c490?auto=format&fit=crop&w=800&q=80" class="img-fluid rounded-4 shadow-lg">
                </div>
                <div class="col-md-6 ps-md-5">
                    <h4 class="fw-bold mb-4">Trang thiết bị</h4>
                    <ul class="list-unstyled svc-list fa-ul">
                        <li><span class="fa-li"><i class="fas fa-tv"></i></span>Smart TV 65 inch</li>
                        <li><span class="fa-li"><i class="fas fa-chalkboard-teacher"></i></span>Bảng kính</li>
                    </ul>
                </div>
            </div>
        `,
    },
    digital: {
        title: "Thư viện Số & Cơ sở Dữ liệu",
        image: "https://atm273446-s3user.vcos.cloudstorage.com.vn/dhdainam/asset/asset/fixed/w88shoi0w4fhr8hwawj620230413082650_thump.jpg",
        content: `
            <p class="lead text-secondary">Truy cập hàng ngàn tài liệu điện tử mọi lúc, mọi nơi.</p>
            <hr class="my-5">
            <div class="row mt-5">
                <div class="col-md-4">
                    <div class="text-center p-4 border rounded-3 hover-shadow">
                        <i class="fas fa-book-reader fa-3x text-primary mb-3"></i>
                        <h5>E-Books</h5>
                        <p class="small text-muted">20,000+ đầu sách điện tử</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center p-4 border rounded-3 hover-shadow">
                        <i class="fas fa-newspaper fa-3x text-success mb-3"></i>
                        <h5>Tạp chí Khoa học</h5>
                        <p class="small text-muted">Kết nối CSDL quốc tế</p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center p-4 border rounded-3 hover-shadow">
                        <i class="fas fa-graduation-cap fa-3x text-warning mb-3"></i>
                        <h5>Luận văn / Đồ án</h5>
                        <p class="small text-muted">Kho tài liệu nội sinh DNU</p>
                    </div>
                </div>
            </div>
        `,
    },
    // --- PHẦN BẠN YÊU CẦU THAY ĐỔI Ở ĐÂY ---
    categories: {
        title: "Tổng quan Tài liệu theo Chuyên ngành",
        image: "https://images.unsplash.com/photo-1550399105-c4db5fb85c18?auto=format&fit=crop&w=1920&q=80",
        content: `
            <div class="px-2">
                <p class="lead text-secondary text-justify">
                    Thư viện Đại Nam sở hữu hệ thống tài liệu phong phú, được xây dựng và cập nhật liên tục bám sát khung chương trình đào tạo thực tế của nhà trường.
                </p>
                
                <hr class="my-4">

                <div class="row">
                    <div class="col-lg-12">
                        <h5 class="fw-bold mb-3" style="color: #2c3e50;">
                            <i class="fas fa-layer-group me-2 text-primary"></i>Phân loại tài liệu
                        </h5>
                        <p class="mb-4 text-secondary">
                            Tài liệu được sắp xếp khoa học, giúp sinh viên dễ dàng tiếp cận tri thức theo từng khối ngành chuyên biệt:
                        </p>

                        <ul class="list-unstyled">
                            <li class="mb-3">
                                <div class="d-flex">
                                    <div class="me-3 mt-1 text-primary"><i class="fas fa-chart-line fa-lg"></i></div>
                                    <div>
                                        <strong class="d-block text-dark">Khối Kinh tế & Quản trị kinh doanh</strong>
                                        <span class="text-muted small">Bao gồm giáo trình Tài chính, Kế toán, Marketing, Quản trị nhân lực và các ấn phẩm kinh tế quốc tế.</span>
                                    </div>
                                </div>
                            </li>
                            
                            <li class="mb-3">
                                <div class="d-flex">
                                    <div class="me-3 mt-1 text-success"><i class="fas fa-laptop-code fa-lg"></i></div>
                                    <div>
                                        <strong class="d-block text-dark">Khối Kỹ thuật & Công nghệ thông tin</strong>
                                        <span class="text-muted small">Cập nhật xu hướng công nghệ mới: AI, Big Data, Lập trình ứng dụng, cùng tài liệu chuyên ngành Ô tô, Điện - Điện tử.</span>
                                    </div>
                                </div>
                            </li>

                            <li class="mb-3">
                                <div class="d-flex">
                                    <div class="me-3 mt-1 text-danger"><i class="fas fa-user-md fa-lg"></i></div>
                                    <div>
                                        <strong class="d-block text-dark">Khối Sức khỏe (Y - Dược)</strong>
                                        <span class="text-muted small">Hệ thống Atlas giải phẫu, Dược điển Việt Nam, sách chuyên khảo Y khoa và tạp chí nghiên cứu uy tín.</span>
                                    </div>
                                </div>
                            </li>

                            <li class="mb-3">
                                <div class="d-flex">
                                    <div class="me-3 mt-1 text-warning"><i class="fas fa-language fa-lg"></i></div>
                                    <div>
                                        <strong class="d-block text-dark">Khối Ngôn ngữ & Khoa học Xã hội</strong>
                                        <span class="text-muted small">Đa dạng đầu sách văn học, lịch sử, văn hóa và giáo trình ngoại ngữ (Anh, Trung, Nhật, Hàn).</span>
                                    </div>
                                </div>
                            </li>
                             <li class="mb-3">
                                <div class="d-flex">
                                    <div class="me-3 mt-1 text-secondary"><i class="fas fa-balance-scale fa-lg"></i></div>
                                    <div>
                                        <strong class="d-block text-dark">Khối Luật học</strong>
                                        <span class="text-muted small">Tuyển tập văn bản quy phạm pháp luật, bình luận án, giáo trình Luật kinh tế, Luật dân sự...</span>
                                    </div>
                                </div>
                            </li>
                        </ul>

                        <div class="alert alert-light border rounded-3 mt-4">
                            <small class="text-muted">
                                <i class="fas fa-info-circle me-1 text-info"></i> 
                                Ngoài ra, sinh viên có thể tra cứu <strong>Luận văn / Đồ án tốt nghiệp</strong> của các khóa trước tại quầy Thông tin.
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        `,
    },
};

function openService(serviceKey) {
    const data = serviceData[serviceKey];
    if (!data) return;

    // Lấy các phần tử DOM
    const header = document.getElementById("svc-header");
    const title = document.getElementById("svc-title");
    const body = document.getElementById("svc-body");
    const overlay = document.getElementById("service-overlay");

    if (header && title && body && overlay) {
        header.style.backgroundImage = `url('${data.image}')`;
        title.innerHTML = data.title;
        body.innerHTML = data.content;

        document.body.style.overflow = "hidden";
        overlay.classList.add("active");
    } else {
        console.error("Không tìm thấy các phần tử HTML cần thiết cho popup (service-overlay, svc-header, v.v...)");
    }
}

function closeService() {
    const overlay = document.getElementById("service-overlay");
    if (overlay) {
        overlay.classList.remove("active");
        document.body.style.overflow = "";
    }
}