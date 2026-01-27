const API_URL = '/api/library/books';
const container = document.getElementById('library-content');
const groupSelect = document.getElementById('group-filter');
let allBooksData = [];

document.addEventListener("DOMContentLoaded", () => {
    loadLibrary();
});

async function loadLibrary() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Error");
        const books = await response.json();
        allBooksData = books;
        populateGroupDropdown(books);
        renderLibrary(books);
    } catch (error) {
        container.innerHTML = `<div style="text-align:center; color:#fab1a0; padding:50px;">Lỗi kết nối vũ trụ tri thức.</div>`;
    }
}

function detectGroup(bookId) {
    if (!bookId || bookId.length < 2) return "Khác";
    const prefix = bookId.substring(0, 2).toUpperCase();
    switch (prefix) {
        case 'IT': return "Công nghệ & Lập trình";
        case 'KH': return "Khoa học Tự nhiên";
        case 'KT': return "Kinh tế & Tài chính";
        case 'NN': return "Ngôn ngữ & Văn hóa";
        case 'VH': return "Văn học & Nghệ thuật";
        default: return "Tài liệu Tổng hợp";
    }
}

function renderLibrary(books) {
    if (!books || books.length === 0) {
        container.innerHTML = '<div style="text-align:center; padding:50px;">Không tìm thấy tín hiệu tài liệu.</div>';
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
            <div class="category-block" style="animation-delay: ${index * 0.2}s">
                <div class="category-header">
                    <h2 class="category-title">${groupName}</h2>
                    <span class="book-count">${groupBooks.length} items</span>
                </div>
                <div class="book-grid">
        `;

        groupBooks.forEach(book => {
            const imgUrl = book.image || "https://via.placeholder.com/300x450/302b63/ffffff?text=DNU+Universe";
            const isAvailable = book.available > 0;
            const description = book.description || "Thông tin đang được cập nhật...";

            html += `
                <div class="book-card">
                    <span class="status-tag ${isAvailable ? 'available' : 'out-of-stock'}">
                        ${isAvailable ? 'Sẵn sàng' : 'Đã hết'}
                    </span>
                    <img src="${imgUrl}" class="book-img">
                    <div class="book-basic-info">
                        <div class="book-title-text">${book.title}</div>
                        <div class="book-author-text">ID: ${book.id}</div>
                    </div>
                    <div class="book-detail-overlay">
                        <div class="overlay-title">${book.title}</div>
                        <div class="overlay-meta"><i class="fas fa-id-badge"></i> ${book.id}</div>
                        <div class="overlay-meta"><i class="fas fa-user-astronaut"></i> ${book.author}</div>
                        <div class="overlay-meta">SL: ${isAvailable ? book.available : 0}</div>
                        <div class="overlay-desc">${description}</div>
                    </div>
                </div>
            `;
        });
        html += `</div></div>`;
        container.innerHTML += html;
    });
}

function populateGroupDropdown(books) {
    const uniqueGroups = new Set();
    books.forEach(book => uniqueGroups.add(detectGroup(book.id)));
    groupSelect.innerHTML = '<option value="all"> Tất cả Khoa</option>';
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
    const filtered = allBooksData.filter(b => b.title.toLowerCase().includes(term) || b.id.toLowerCase().includes(term));
    renderLibrary(filtered);
    groupSelect.value = 'all';
};