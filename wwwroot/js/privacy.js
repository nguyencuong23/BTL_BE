const API_URL = '/api/library/books';
const container = document.getElementById('library-content');
const groupSelect = document.getElementById('group-filter');
let allBooksData = [];

// --- 1. KHỞI TẠO ---
document.addEventListener("DOMContentLoaded", () => {
    loadLibrary();
    setupModalEvents();
});

// --- 2. FETCH API ---
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
                <i class="fas fa-wifi" style="font-size: 3rem; margin-bottom: 15px;"></i>
                <h3>Mất kết nối</h3>
                <p>Không thể tải dữ liệu từ máy chủ.</p>
            </div>`;
    }
}

// --- 3. HELPER: PHÂN LOẠI SÁCH ---
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

// --- 4. HELPER: TẠO MÔ TẢ TỰ ĐỘNG (SMART DESCRIPTION) ---
function getSmartDescription(book) {
    // 1. Nếu API có sẵn mô tả thì dùng luôn
    if (book.description && book.description.trim().length > 10) {
        return book.description;
    }

    // 2. Tự động tạo nội dung theo Thể loại (Group)
    const groupName = detectGroup(book.id);

    let template = "";
    switch (groupName) {
        case "Công nghệ & Lập trình":
            template = `Tài liệu <b>"${book.title}"</b> là nguồn kiến thức quý giá dành cho sinh viên và kỹ sư ngành CNTT. Sách cung cấp các nền tảng kỹ thuật quan trọng, cập nhật xu hướng công nghệ mới giúp người đọc nâng cao tư duy lập trình và giải quyết vấn đề hiệu quả.`;
            break;
        case "Kinh tế & Tài chính":
            template = `Cuốn sách <b>"${book.title}"</b> của tác giả ${book.author} đi sâu phân tích các nguyên lý kinh tế thị trường, quản trị tài chính và chiến lược kinh doanh. Đây là tài liệu tham khảo thiết yếu, giúp sinh viên khối ngành Kinh tế nắm bắt tư duy quản lý hiện đại.`;
            break;
        case "Ngôn ngữ & Văn hóa":
            template = `Khám phá vẻ đẹp ngôn ngữ và bản sắc văn hóa đa dạng qua tác phẩm <b>"${book.title}"</b>. Sách không chỉ giúp người đọc mở rộng vốn từ vựng, ngữ pháp mà còn cung cấp cái nhìn sâu sắc về bối cảnh văn hóa đặc trưng.`;
            break;
        case "Y Dược & Sức khỏe":
            template = `Tài liệu chuyên ngành Y Dược cung cấp kiến thức y khoa chính xác, cập nhật các phương pháp chẩn đoán và điều trị tiên tiến. <b>"${book.title}"</b> là cẩm nang hữu ích hỗ trợ đắc lực cho sinh viên y khoa và các bác sĩ trong quá trình nghiên cứu.`;
            break;
        case "Khoa học Tự nhiên":
            template = `Tài liệu <b>"${book.title}"</b> tập hợp các nghiên cứu và lý thuyết nền tảng về khoa học tự nhiên. Sách được trình bày logic, khoa học, phù hợp cho việc tra cứu và nghiên cứu chuyên sâu của giảng viên và sinh viên.`;
            break;
        case "Pháp luật & Đời sống":
            template = `Cuốn sách hệ thống hóa các quy định pháp luật hiện hành và các tình huống thực tiễn. <b>"${book.title}"</b> là tài liệu quan trọng giúp người đọc hiểu rõ hơn về hệ thống pháp lý và cách áp dụng luật vào đời sống.`;
            break;
        default:
            template = `Tác phẩm <b>"${book.title}"</b> được biên soạn bởi tác giả <b>${book.author}</b>. Đây là một trong những tài liệu được lưu trữ cẩn thận tại thư viện, phục vụ nhu cầu học tập và nghiên cứu đa dạng của sinh viên và giảng viên nhà trường. Mời bạn đọc mượn sách để tìm hiểu chi tiết nội dung.`;
            break;
    }
    return template;
}

// --- 5. RENDER GRID SÁCH ---
function renderLibrary(books) {
    if (!books || books.length === 0) {
        container.innerHTML = `
            <div style="text-align:center; padding:50px; color: var(--text-muted);">
                <i class="fas fa-search" style="font-size: 2rem; margin-bottom:10px; opacity:0.5;"></i>
                <p>Không tìm thấy sách phù hợp.</p>
            </div>`;
        return;
    }

    const groups = books.reduce((acc, book) => {
        const groupName = detectGroup(book.id);
        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push(book);
        return acc;
    }, {});

    const sortedGroupNames = Object.keys(groups).sort();
    container.innerHTML = '';

    sortedGroupNames.forEach((groupName, index) => {
        const groupBooks = groups[groupName];
        let html = `
            <div class="category-block" style="animation-delay: ${index * 0.1}s">
                <div class="category-header">
                    <h2 class="category-title">${groupName}</h2>
                    <span class="book-count">${groupBooks.length} Cuốn</span>
                </div>
                <div class="book-grid">`;

        groupBooks.forEach(book => {
            const imgUrl = book.image && book.image.trim() !== '' ? book.image : "https://via.placeholder.com/300x450/005aab/ffffff?text=DNU+Library";
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
                </div>`;
        });
        html += `</div></div>`;
        container.innerHTML += html;
    });
}

// --- 6. HELPER: CHUẨN HÓA DỮ LIỆU ---
function getBookDetails(book) {
    return {
        publisher: book.publisher || "NXB Giáo Dục",
        year: book.year || "2023",
        pages: book.pages || "Đang cập nhật",
        isbn: book.isbn || `978-604-${Math.floor(Math.random() * 1000)}-X`,
        language: book.language || "Tiếng Việt",
        location: book.location || `Kệ ${book.id.substring(0, 2).toUpperCase()} - Tầng 2`,
        category: detectGroup(book.id)
    };
}

// --- 7. LOGIC MỞ MODAL CHI TIẾT ---
function openDetailModal(bookId) {
    const book = allBooksData.find(b => b.id === bookId);
    if (!book) return;

    const modal = document.getElementById('modal-overlay');
    const contentDiv = document.getElementById('detail-content');

    // Dữ liệu hiển thị
    const meta = getBookDetails(book);
    const imgUrl = book.image && book.image.trim() !== '' ? book.image : "https://via.placeholder.com/300x450";

    // Sử dụng hàm tạo mô tả thông minh
    const description = getSmartDescription(book);

    // Trạng thái
    const isAvailable = book.available > 0;
    const statusColor = isAvailable ? '#059669' : '#dc2626';
    const statusBg = isAvailable ? '#ecfdf5' : '#fef2f2';
    const statusIcon = isAvailable ? 'fa-check-circle' : 'fa-times-circle';
    const statusText = isAvailable ? `Sẵn sàng (${book.available})` : 'Hết sách';

    contentDiv.innerHTML = `
        <div style="display: flex; flex-direction: row; gap: 30px; align-items: flex-start; width: 100%;">
            
            <div style="flex: 0 0 260px; max-width: 260px; text-align: center;">
                <div style="border-radius: 12px; overflow: hidden; box-shadow: 0 10px 20px rgba(0,0,0,0.2); background: #fff;">
                    <img src="${imgUrl}" alt="${book.title}" 
                         style="display: block; width: 100%; height: auto; object-fit: cover;">
                </div>
            </div>

            <div style="flex: 1; min-width: 0;">
                <h2 style="margin: 0 0 10px 0; font-size: 1.8rem; color: #004e92; line-height: 1.3;">${book.title}</h2>
                
                <div style="margin-bottom: 20px; color: #475569; font-size: 1.1rem;">
                    <i class="fas fa-pen-nib"></i> Tác giả: <strong>${book.author}</strong>
                </div>

                <div style="display: flex; gap: 15px; margin-bottom: 25px; border-bottom: 1px dashed #e2e8f0; padding-bottom: 15px;">
                    <span style="background: ${statusBg}; color: ${statusColor}; padding: 6px 15px; border-radius: 20px; font-weight: 600; font-size: 0.9rem; display: inline-flex; align-items: center; gap: 6px;">
                        <i class="fas ${statusIcon}"></i> ${statusText}
                    </span>
                </div>

                <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px 30px; background: #f8fafc; padding: 15px; border-radius: 10px; margin-bottom: 20px; font-size: 0.95rem;">
                    <div><span style="color:#94a3b8; font-size:0.8rem; display:block;">THỂ LOẠI</span> <b style="color:#334155;">${meta.category}</b></div>
                    <div><span style="color:#94a3b8; font-size:0.8rem; display:block;">NXB</span> <b style="color:#334155;">${meta.publisher}</b></div>
                    <div><span style="color:#94a3b8; font-size:0.8rem; display:block;">NĂM XB</span> <b style="color:#334155;">${meta.year}</b></div>
                    <div><span style="color:#94a3b8; font-size:0.8rem; display:block;">NGÔN NGỮ</span> <b style="color:#334155;">${meta.language}</b></div>
                </div>

                <div>
                    <label style="color: #005aab; font-weight: bold; display: block; margin-bottom: 8px;">
                        <i class="fas fa-align-left"></i> Giới thiệu nội dung:
                    </label>
                    <div style="max-height: 200px; overflow-y: auto; background: #fff; border: 1px solid #e2e8f0; padding: 15px; border-radius: 8px; line-height: 1.6; color: #334155; text-align: justify;">
                        ${description}
                    </div>
                </div>
            </div>
        </div>
    `;

    modal.classList.add('active');
}

// --- 8. EVENTS & FILTER ---
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
    const filtered = allBooksData.filter(b => b.title.toLowerCase().includes(term) || b.author.toLowerCase().includes(term));
    renderLibrary(filtered);
    groupSelect.value = 'all';
};