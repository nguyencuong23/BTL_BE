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
                    Không thể tải dữ liệu.<br>
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

    Object.keys(groups).sort().forEach((groupName) => {
        const groupBooks = groups[groupName];
        const headerDiv = document.createElement('div');
        headerDiv.className = 'category-header';
        headerDiv.innerHTML = `<h2 class="category-title">${groupName}</h2><span class="book-count">${groupBooks.length} Cuốn</span>`;
        grid.appendChild(headerDiv);

        groupBooks.forEach(book => {
            const isAvail = book.available > 0;
            const img = book.image || "https://via.placeholder.com/300x450/302b63/ffffff?text=DNU";
            const card = document.createElement('div');
            card.className = 'book-card';
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
                </div>`;
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
    if (selectedGroup === 'all') render(allBooksData);
    else render(allBooksData.filter(book => detectGroup(book.id) === selectedGroup));
};

window.filterBooks = () => {
    const searchInput = document.getElementById('search-input');
    if (!searchInput) return;
    const term = searchInput.value.toLowerCase().trim();
    render(allBooksData.filter(b => b.title.toLowerCase().includes(term)));
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
        setupDate();
        borrowModal.classList.add('active');
    }
};

window.onclick = (e) => {
    if (borrowModal && e.target == borrowModal) borrowModal.classList.remove('active');
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
    btn.innerText = 'Đang xử lý...';
    btn.disabled = true;

    const getVal = (id) => document.getElementById(id) ? document.getElementById(id).value : "";

    const payload = {
        BookId: getVal('book-id'),
        StudentId: getVal('student-id'),
        FullName: getVal('full-name'),
        ReturnDate: getVal('return-date')
    };

    try {
        const res = await fetch(`${API_URL}/borrow`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert("Đăng ký thành công! Vui lòng đến thư viện nhận sách.");
            if (borrowModal) borrowModal.classList.remove('active');
            form.reset();
            fetchBooks();
        } else {
            const txt = await res.text();
            try {
                const errObj = JSON.parse(txt);
                alert("Thông báo: " + (errObj.message || txt));
            } catch {
                console.error("Server Error:", txt);
                if (txt.includes("duplicate key")) {
                    alert("Lỗi: Mã sinh viên này đã tồn tại nhưng thông tin bị trùng lặp. Vui lòng liên hệ thủ thư.");
                } else {
                    alert("Có lỗi xảy ra: " + txt);
                }
            }
        }
    } catch (err) {
        alert("Không thể kết nối đến máy chủ.");
    } finally {
        btn.innerText = oldText;
        btn.disabled = false;
    }
}

function setupDate() {
    const borrowDateInput = document.getElementById('borrow-date');
    const dueLimitInput = document.getElementById('due-limit-date');
    const returnDateInput = document.getElementById('return-date');

    if (borrowDateInput && dueLimitInput && returnDateInput) {
        const formatDate = (date) => {
            const y = date.getFullYear();
            const m = String(date.getMonth() + 1).padStart(2, '0');
            const d = String(date.getDate()).padStart(2, '0');
            return `${y}-${m}-${d}`;
        };

        const today = new Date();
        const todayStr = formatDate(today);

        borrowDateInput.value = todayStr;

        const limitDate = new Date(today);
        limitDate.setDate(limitDate.getDate() + 14);
        const limitStr = formatDate(limitDate);
        dueLimitInput.value = limitStr;

        returnDateInput.min = todayStr;
        returnDateInput.max = limitStr;

        const tomorrow = new Date(today);
        tomorrow.setDate(tomorrow.getDate() + 1);

        if (tomorrow > limitDate) {
            returnDateInput.value = limitStr;
        } else {
            returnDateInput.value = formatDate(tomorrow);
        }
    }
}

function handleInstructionModal() {
    const instructionModal = document.getElementById('instruction-modal');
    const instructionBtn = document.getElementById('btn-close-instruction');
    if (instructionModal) {
        document.body.style.overflow = 'hidden';
        setTimeout(() => instructionModal.classList.add('active'), 300);
        if (instructionBtn) {
            instructionBtn.onclick = () => {
                instructionModal.classList.remove('active');
                document.body.style.overflow = 'auto';
            };
        }
    }
}