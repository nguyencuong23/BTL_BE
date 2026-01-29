/* ==========================================================
   FILE: wwwroot/js/tracuu.js
   MÔ TẢ: Xử lý tìm kiếm, lọc kệ, và hiển thị chi tiết sách
   ========================================================== */

const API_URL = '/api/library/books';
let allBooksData = [];

// 1. KHỞI TẠO KHI LOAD TRANG
document.addEventListener("DOMContentLoaded", () => {
    loadLibraryData();
    setupModalEvents();
});

// 2. TẢI DỮ LIỆU TỪ API
async function loadLibraryData() {
    const loadingState = document.getElementById('loadingState');
    const resultGrid = document.getElementById('resultGrid');
    const emptyState = document.getElementById('emptyState');

    loadingState.style.display = 'block';
    resultGrid.innerHTML = '';
    emptyState.style.display = 'none';

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Lỗi tải dữ liệu");

        const books = await response.json();
        allBooksData = books;

        // Tự động đổ dữ liệu vào Dropdown Kệ sách
        populateShelfDropdown(allBooksData);

        // Hiển thị danh sách ban đầu
        renderGrid(allBooksData);
    } catch (error) {
        console.error(error);
        resultGrid.innerHTML = `
            <div class="col-12 text-center text-danger py-5">
                <i class="fas fa-satellite-dish fa-3x mb-3"></i>
                <p>Mất kết nối với máy chủ thư viện.</p>
            </div>`;
    } finally {
        loadingState.style.display = 'none';
    }
}

// 3. TẠO DROPDOWN KỆ SÁCH (TỰ ĐỘNG)
function populateShelfDropdown(books) {
    const shelfSelect = document.getElementById('shelfSelect');
    if (!shelfSelect) return; // Phòng trường hợp không có dropdown

    // Lấy danh sách vị trí duy nhất
    const uniqueShelves = [...new Set(books.map(b => b.location || "Chưa xếp kệ"))].sort();

    // Xóa các option cũ (trừ option đầu tiên là "Tất cả")
    while (shelfSelect.options.length > 1) {
        shelfSelect.remove(1);
    }

    uniqueShelves.forEach(shelf => {
        const option = document.createElement('option');
        option.value = shelf;
        option.textContent = shelf;
        shelfSelect.appendChild(option);
    });
}

// 4. HÀM PHỤ TRỢ: ĐOÁN THỂ LOẠI TỪ MÃ
function detectGroup(bookId) {
    if (!bookId || bookId.length < 2) return "Tài liệu Tổng hợp";
    const prefix = bookId.substring(0, 2).toUpperCase();
    const groups = {
        'IT': "CNTT & Lập trình",
        'KH': "Khoa học Tự nhiên",
        'KT': "Kinh tế & Tài chính",
        'NN': "Ngôn ngữ",
        'VH': "Văn học",
        'PL': "Pháp luật",
        'YD': "Y Dược"
    };
    return groups[prefix] || "Tài liệu khác";
}

// 5. TÌM KIẾM NHANH (TAGS)
function quickSearch(text) {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.value = text;
        handleDocSearch();
    }
}

// 6. XỬ LÝ TÌM KIẾM & LỌC
function handleDocSearch(event) {
    if (event) event.preventDefault();

    const searchInput = document.getElementById('searchInput');
    const shelfSelect = document.getElementById('shelfSelect');

    const keyword = searchInput ? searchInput.value.toLowerCase().trim() : "";
    const selectedShelf = shelfSelect ? shelfSelect.value : "all";

    const loadingState = document.getElementById('loadingState');
    const resultGrid = document.getElementById('resultGrid');

    resultGrid.innerHTML = '';
    if (loadingState) loadingState.style.display = 'block';

    setTimeout(() => {
        const filtered = allBooksData.filter(book => {
            // Lọc theo từ khóa
            const matchKeyword = book.title.toLowerCase().includes(keyword) ||
                book.author.toLowerCase().includes(keyword);

            // Lọc theo Kệ sách
            let matchShelf = true;
            if (selectedShelf !== 'all') {
                const bookLoc = (book.location && book.location !== "null") ? book.location : "Chưa xếp kệ";
                matchShelf = bookLoc === selectedShelf;
            }

            return matchKeyword && matchShelf;
        });

        renderGrid(filtered);
        if (loadingState) loadingState.style.display = 'none';
    }, 300);

    return false;
}

// 7. RENDER DANH SÁCH SÁCH (GRID)
function renderGrid(books) {
    const grid = document.getElementById('resultGrid');
    const emptyState = document.getElementById('emptyState');

    if (!books || books.length === 0) {
        if (emptyState) emptyState.style.display = 'block';
        return;
    } else {
        if (emptyState) emptyState.style.display = 'none';
    }

    let html = '';
    books.forEach(book => {
        const isAvailable = book.available > 0;
        const badgeHtml = isAvailable
            ? `<span class="book-badge badge-avail"><i class="fa-solid fa-check"></i> Sẵn sàng</span>`
            : `<span class="book-badge badge-out"><i class="fa-solid fa-xmark"></i> Hết sách</span>`;

        const imgUrl = book.image && book.image.trim() !== ''
            ? book.image
            : "https://via.placeholder.com/300x450?text=No+Image";

        const categoryName = detectGroup(book.id);
        const locationDisplay = (book.location && book.location !== "null") ? book.location : "Đang cập nhật";

        const locationBadge = `<span class="location-badge"><i class="fa-solid fa-location-dot"></i> ${locationDisplay}</span>`;

        html += `
            <div class="col-lg-3 col-md-4 col-sm-6 animate-fade-in">
                <div class="book-card" onclick="openDetailModal('${book.id}')">
                    <div class="book-cover-wrapper">
                        ${badgeHtml}
                        <img src="${imgUrl}" class="book-cover" alt="${book.title}" loading="lazy">
                        <div class="book-overlay">
                            <span>Xem chi tiết</span>
                        </div>
                    </div>
                    <div class="book-info">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <small class="text-muted book-cat">${categoryName}</small>
                            ${locationBadge}
                        </div>
                        <h3 class="book-title" title="${book.title}">${book.title}</h3>
                        <p class="book-author"><i class="fa-solid fa-pen-nib"></i> ${book.author}</p>
                    </div>
                </div>
            </div>
        `;
    });

    grid.innerHTML = html;
}

// 8. HIỂN THỊ MODAL CHI TIẾT (Đã fix lỗi cú pháp)
function openDetailModal(bookId) {
    const book = allBooksData.find(b => b.id === bookId);
    if (!book) return;

    const modalOverlay = document.getElementById('modal-overlay');
    const contentDiv = document.getElementById('detail-content');

    const imgUrl = book.image || "https://via.placeholder.com/300x450";

    // 1. Tự động suy ra thể loại
    const categoryName = detectGroup(book.id);

    // 2. Xử lý hiển thị trạng thái
    const statusText = book.available > 0
        ? `<span class="text-success fw-bold"><i class="fas fa-check-circle"></i> Có sẵn (${book.available} cuốn)</span>`
        : `<span class="text-danger fw-bold"><i class="fas fa-times-circle"></i> Đã hết sách</span>`;

    // 3. Xử lý hiển thị vị trí
    const locationDisplay = (book.location && book.location !== "null") ? book.location : "Đang cập nhật";

    // 4. TẠO MÔ TẢ TỰ ĐỘNG (Sử dụng Template Literal chuẩn)
    const autoDescription = `
        <p><strong>${book.title}</strong> là tài liệu thuộc nhóm chuyên ngành <strong>${categoryName}</strong>, được biên soạn bởi tác giả <strong>${book.author}</strong>.</p>
        
        <p>Đây là tài liệu tham khảo quan trọng dành cho sinh viên và giảng viên trong quá trình học tập, nghiên cứu. Hiện tại thư viện đang lưu trữ và quản lý tài liệu này tại khu vực <strong>${locationDisplay}</strong>.</p>
        
        <ul class="list-unstyled mt-3" style="background: #f8f9fa; padding: 10px; border-radius: 8px; font-size: 0.9rem;">
            <li><i class="fa-solid fa-layer-group text-primary"></i> <strong>Phân loại:</strong> ${categoryName}</li>
            <li><i class="fa-solid fa-building-columns text-primary"></i> <strong>Thư viện:</strong> Đại học DNU</li>
        </ul>
    `;

    // 5. Render HTML vào Modal
    contentDiv.innerHTML = `
        <div class="modal-content-flex">
            <div class="modal-img-col">
                <img src="${imgUrl}" class="modal-book-img" alt="${book.title}">
            </div>
            <div class="modal-info-col">
                <h2 class="modal-book-title">${book.title}</h2>
                
                <div class="modal-meta-grid">
                    <div class="meta-item">
                        <label>Tác giả</label>
                        <span>${book.author}</span>
                    </div>
                    <div class="meta-item">
                        <label>Vị trí kệ</label>
                        <span class="text-primary fw-bold"><i class="fa-solid fa-map-pin"></i> ${locationDisplay}</span>
                    </div>
                    <div class="meta-item">
                        <label>Trạng thái</label>
                        <span>${statusText}</span>
                    </div>
                </div>

                <hr style="opacity: 0.1">

                <div class="modal-book-desc">
                    <h6 class="text-uppercase fw-bold text-secondary mb-3" style="font-size: 0.8rem; letter-spacing: 1px;">Thông tin chi tiết</h6>
                    ${autoDescription}
                </div>
                
                <div class="modal-actions mt-4 d-flex gap-2">
                     <a href="/Client/Paypack?bookId=${book.id}" class="btn btn-primary flex-grow-1 py-2 rounded-pill fw-bold text-decoration-none d-flex align-items-center justify-content-center">
                        <i class="fa-solid fa-book-reader me-2"></i> Đăng ký mượn ngay
                     </a>

                     <button class="btn btn-outline-secondary rounded-circle" style="width: 45px; height: 45px;" onclick="closeModal()">
                        <i class="fa-solid fa-xmark"></i>
                     </button>
                </div>
            </div>
        </div>
    `;

    modalOverlay.classList.add('active');
}

// 9. CÁC HÀM SỰ KIỆN MODAL
function closeModal() {
    const modalOverlay = document.getElementById('modal-overlay');
    if (modalOverlay) modalOverlay.classList.remove('active');
}

function setupModalEvents() {
    const modalOverlay = document.getElementById('modal-overlay');
    const closeBtn = document.querySelector('.close-btn');

    if (closeBtn) closeBtn.onclick = closeModal;

    if (modalOverlay) {
        modalOverlay.onclick = (e) => {
            if (e.target === modalOverlay) closeModal();
        };
    }
}