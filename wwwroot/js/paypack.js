const API_URL = '/api/library';

let allBooksData = [];

let grid, borrowModal, borrowCloseBtn, form, groupSelect;

document.addEventListener("DOMContentLoaded", function () {
    grid = document.getElementById('book-grid');
    borrowModal = document.getElementById('modal-overlay');
    borrowCloseBtn = document.querySelector('#modal-overlay .close-btn');
    form = document.getElementById('borrow-form');
    groupSelect = document.getElementById('group-filter');

    handleInstructionModal();
    setupDate();

    fetchBooks();

    if (borrowCloseBtn) {
        borrowCloseBtn.onclick = () => {
            if (borrowModal) borrowModal.classList.remove('active');
        };
    }

    if (form) {
        form.addEventListener('submit', handleFormSubmit);
    }
});

async function fetchBooks() {
    if (grid) grid.innerHTML = '<div style="text-align:center; padding:50px; grid-column:1/-1; color:white;">Đang kết nối đến máy chủ...</div>';

    try {
        const res = await fetch(`${API_URL}/books`);
        if (!res.ok) throw new Error(`Lỗi HTTP: ${res.status}`);

        const data = await res.json();

        allBooksData = data;

        if (groupSelect) populateGroupDropdown(allBooksData);

        render(allBooksData);

    } catch (e) {
        console.error(e);
        if (grid) {
            grid.innerHTML = `
                <div style="text-align:center; padding:50px; color: #ef4444; grid-column: 1/-1;">
                    <i class="fas fa-exclamation-triangle"></i><br>
                    Không thể tải dữ liệu. Vui lòng kiểm tra API.<br>
                    <small>${e.message}</small>
                </div>`;
        }
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

function render(books) {
    if (!grid) return;
    grid.innerHTML = '';

    if (!books || books.length === 0) {
        grid.innerHTML = '<div style="text-align:center; color: #64748b; padding:20px; grid-column: 1/-1;">Không tìm thấy sách nào phù hợp.</div>';
        return;
    }

    const groups = books.reduce((acc, book) => {
        const groupName = detectGroup(book.id);
        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push(book);
        return acc;
    }, {});

    const sortedGroupNames = Object.keys(groups).sort();

    sortedGroupNames.forEach((groupName) => {
        const groupBooks = groups[groupName];

        const headerDiv = document.createElement('div');
        headerDiv.className = 'category-header';
        headerDiv.innerHTML = `
            <h2 class="category-title">${groupName}</h2>
            <span class="book-count">${groupBooks.length} Cuốn</span>
        `;
        grid.appendChild(headerDiv);

        groupBooks.forEach(book => {
            const isAvail = book.available > 0;
            const img = book.image || "https://via.placeholder.com/300x450/302b63/ffffff?text=DNU";

            const card = document.createElement('div');
            card.className = 'book-card';

            // Cập nhật HTML: Thêm hiển thị tác giả vào cả overlay
            card.innerHTML = `
                <img src="${img}" class="book-img" onerror="this.src='https://via.placeholder.com/300x450?text=Error'">
                
                <div class="book-meta">
                    <div class="book-title">${book.title}</div>
                    <div class="book-author">${book.author}</div>
                </div>

                <div class="book-overlay">
                    <h3 style="font-family:'Playfair Display'; color:#ffeaa7; margin-bottom: 5px;">${book.title}</h3>
                    
                    <p style="color: #e2e8f0; font-style: italic; margin-bottom: 15px; font-size: 0.9rem;">
                        <i class="fas fa-pen-fancy"></i> ${book.author}
                    </p>

                    <button class="btn-borrow" ${!isAvail ? 'disabled' : ''} onclick="openModal('${book.id}', '${book.title}')">
                        ${isAvail ? 'Mượn Ngay' : 'Đã Hết'}
                    </button>
                </div>
            `;
            grid.appendChild(card);
        });
    });
}

function populateGroupDropdown(books) {
    if (!groupSelect) return;
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
    if (!groupSelect) return;
    const selectedGroup = groupSelect.value;

    if (selectedGroup === 'all') {
        render(allBooksData);
    } else {
        const filtered = allBooksData.filter(book => detectGroup(book.id) === selectedGroup);
        render(filtered);
    }
};

window.filterBooks = () => {
    const searchInput = document.getElementById('search-input');
    if (!searchInput) return;

    const term = searchInput.value.toLowerCase().trim();
    const filtered = allBooksData.filter(b => b.title.toLowerCase().includes(term));
    render(filtered);

    if (groupSelect) groupSelect.value = 'all';
};

window.openModal = (id, title) => {
    if (!borrowModal) borrowModal = document.getElementById('modal-overlay');

    if (borrowModal) {
        document.getElementById('book-id').value = id;
        const highlight = document.getElementById('book-highlight');
        if (highlight) {
            highlight.innerHTML = `<p style="color:var(--dnu-blue); text-align:center; margin-bottom:20px;">Sách chọn: <strong>${title}</strong></p>`;
        }
        borrowModal.classList.add('active');
    }
};

window.onclick = (e) => {
    if (borrowModal && e.target == borrowModal) {
        borrowModal.classList.remove('active');
    }
    const instructionModal = document.getElementById('instruction-modal');
    if (instructionModal && e.target == instructionModal) {
        instructionModal.classList.remove('active');
        document.body.style.overflow = 'auto';
    }
}

async function handleFormSubmit(e) {
    e.preventDefault();
    const btn = document.querySelector('.btn-submit');
    const oldText = btn.innerText;

    btn.innerText = 'Đang gửi...';
    btn.disabled = true;

    const payload = {
        BookId: document.getElementById('book-id').value,
        StudentId: document.getElementById('student-id').value,
        ReturnDate: document.getElementById('return-date').value
    };

    try {
        const res = await fetch(`${API_URL}/borrow`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert("Đăng ký thành công!");
            if (borrowModal) borrowModal.classList.remove('active');
            form.reset();
            fetchBooks();
        } else {
            const txt = await res.text();
            alert("Lỗi: " + txt);
        }
    } catch (err) {
        alert("Lỗi kết nối đến server.");
    } finally {
        btn.innerText = oldText;
        btn.disabled = false;
    }
}

function setupDate() {
    const returnDateInput = document.getElementById('return-date');
    if (returnDateInput) {
        const dt = new Date();
        dt.setDate(dt.getDate() + 1);
        returnDateInput.min = dt.toISOString().split('T')[0];
    }
}

function handleInstructionModal() {
    const instructionModal = document.getElementById('instruction-modal');
    const instructionBtn = document.getElementById('btn-close-instruction');

    if (instructionModal) {
        document.body.style.overflow = 'hidden';
        setTimeout(() => {
            instructionModal.classList.add('active');
        }, 300);

        if (instructionBtn) {
            instructionBtn.onclick = function () {
                instructionModal.classList.remove('active');
                document.body.style.overflow = 'auto';
            };
        }
    }
}