const API_URL = '/api/library/books';
const container = document.getElementById('library-content');
const groupSelect = document.getElementById('group-filter');
let allBooksData = [];

// 1. Khởi tạo
document.addEventListener("DOMContentLoaded", () => {
    loadLibrary();
    setupModalEvents();
});

// 2. Fetch API
async function loadLibrary() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Error fetching data");
        const books = await response.json();
        allBooksData = books;

        populateGroupDropdown(books);
        renderLibrary(books);
    } catch (error) {
        console.error(error);
        container.innerHTML = `
            <div style="text-align:center; padding:50px; color: #ef4444;">
                <i class="fas fa-satellite-dish" style="font-size: 3rem; margin-bottom: 15px;"></i>
                <h3>Mất tín hiệu kết nối</h3>
                <p>Không thể tải dữ liệu từ máy chủ.</p>
            </div>`;
    }
}

// 3. Phân loại sách
function detectGroup(bookId) {
    if (!bookId || bookId.length < 2) return "Tài liệu Khác";
    const prefix = bookId.substring(0, 2).toUpperCase();
    const groups = {
        'IT': "Công nghệ & Lập trình",
        'KH': "Khoa học Tự nhiên",
        'KT': "Kinh tế & Tài chính",
        'NN': "Ngôn ngữ & Văn hóa",
        'VH': "Văn học & Nghệ thuật",
        'PL': "Pháp luật & Đời sống",
        'YD': "Y Dược & Sức khỏe"
    };
    return groups[prefix] || "Tài liệu Tổng hợp";
}

// 4. Render Grid Sách
function renderLibrary(books) {
    if (!books || books.length === 0) {
        container.innerHTML = `
            <div style="text-align:center; padding:50px; color: var(--text-muted);">
                <i class="fas fa-search" style="font-size: 2rem; margin-bottom:10px; opacity:0.5;"></i>
                <p>Không tìm thấy cuốn sách nào phù hợp.</p>
            </div>`;
        return;
    }

    // Nhóm sách
    const groups = books.reduce((acc, book) => {
        const groupName = detectGroup(book.id);
        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push(book);
        return acc;
    }, {});

    const sortedGroupNames = Object.keys(groups).sort();
    container.innerHTML = '';

    // Render từng nhóm
    sortedGroupNames.forEach((groupName, index) => {
        const groupBooks = groups[groupName];
        let html = `
            <div class="category-block" style="animation-delay: ${index * 0.1}s">
                <div class="category-header">
                    <h2 class="category-title">${groupName}</h2>
                    <span class="book-count">${groupBooks.length} Cuốn</span>
                </div>
                <div class="book-grid">
        `;

        groupBooks.forEach(book => {
            const imgUrl = book.image && book.image.trim() !== ''
                ? book.image
                : "https://via.placeholder.com/300x450/005aab/ffffff?text=DNU+Library";

            const isAvailable = book.available > 0;
            const statusClass = isAvailable ? 'available' : 'out-of-stock';
            const statusText = isAvailable ? 'Sẵn sàng' : 'Hết sách';

            html += `
                <div class="book-card" onclick="openDetailModal('${book.id}')">
                    <span class="status-tag ${statusClass}">${statusText}</span>
                    <img src="${imgUrl}" class="book-img" alt="${book.title}" loading="lazy">
                    
                    <div class="book-meta">
                        <div class="book-title" title="${book.title}">${book.title}</div>
                        <div class="book-author">${book.author}</div>
                    </div>

                    <div class="book-overlay">
                        <button class="btn-borrow"><i class="fas fa-eye"></i> Xem Chi Tiết</button>
                    </div>
                </div>
            `;
        });

        html += `</div></div>`;
        container.innerHTML += html;
    });
}

// 5. Logic Mở Modal Chi Tiết
function openDetailModal(bookId) {
    const book = allBooksData.find(b => b.id === bookId);
    if (!book) return;

    const modal = document.getElementById('modal-overlay');
    const contentDiv = document.getElementById('detail-content');

    // Xử lý dữ liệu
    const imgUrl = book.image && book.image.trim() !== '' ? book.image : "https://via.placeholder.com/300x450";
    const description = book.description || "Chưa có mô tả chi tiết cho tài liệu này.";
    const isAvailable = book.available > 0;
    const statusLabel = isAvailable
        ? `<span style="color: #10b981; font-weight:bold;"><i class="fas fa-check-circle"></i> Còn ${book.available} cuốn</span>`
        : `<span style="color: #ef4444; font-weight:bold;"><i class="fas fa-times-circle"></i> Đã hết sách</span>`;

    // Inject HTML chi tiết
    contentDiv.innerHTML = `
        <img src="${imgUrl}" class="modal-book-img" alt="${book.title}">
        <div class="modal-book-info">
            <h2 class="modal-book-title">${book.title}</h2>
            
            <div class="modal-book-meta">
                <p><i class="fas fa-user-pen" style="color:var(--dnu-orange)"></i> Tác giả: <strong>${book.author}</strong></p>
                <p><i class="fas fa-bookmark" style="color:var(--dnu-orange)"></i> Thể loại: ${detectGroup(book.id)}</p>
                <p>${statusLabel}</p>
            </div>

            <label style="font-weight:bold; color:var(--dnu-blue); margin-bottom:5px;">Giới thiệu nội dung:</label>
            <div class="modal-book-desc">
                ${description}
            </div>
        </div>
    `;

    modal.classList.add('active');
}

// 6. Logic Đóng/Mở Modal & Tìm kiếm
function setupModalEvents() {
    const modalOverlay = document.getElementById('modal-overlay');
    const closeBtn = document.querySelector('.close-btn');

    const closeFunc = () => modalOverlay.classList.remove('active');

    if (closeBtn) closeBtn.onclick = closeFunc;
    if (modalOverlay) {
        modalOverlay.onclick = (e) => {
            if (e.target === modalOverlay) closeFunc();
        };
    }
    window.closeModalFunc = closeFunc;
}

function populateGroupDropdown(books) {
    const uniqueGroups = new Set();
    books.forEach(book => uniqueGroups.add(detectGroup(book.id)));
    groupSelect.innerHTML = '<option value="all">Tất cả Khoa</option>';
    Array.from(uniqueGroups).sort().forEach(groupName => {
        const option = document.createElement('option');
        option.value = groupName;
        option.innerText = groupName;
        groupSelect.appendChild(option);
    });
}

window.filterByGroup = () => {
    const selectedGroup = groupSelect.value;
    if (selectedGroup === 'all') {
        renderLibrary(allBooksData);
    } else {
        const filtered = allBooksData.filter(book => detectGroup(book.id) === selectedGroup);
        renderLibrary(filtered);
    }
};

window.searchBooks = () => {
    const term = document.getElementById('search-input').value.toLowerCase().trim();
    if (!term) { renderLibrary(allBooksData); return; }
    const filtered = allBooksData.filter(b =>
        b.title.toLowerCase().includes(term) || b.author.toLowerCase().includes(term)
    );
    renderLibrary(filtered);
    groupSelect.value = 'all';
};