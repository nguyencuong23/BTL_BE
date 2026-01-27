const API_URL = '/api/library';
const grid = document.getElementById('book-grid');
const modal = document.getElementById('modal-overlay');
const closeBtn = document.querySelector('.close-btn');
const form = document.getElementById('borrow-form');
const groupSelect = document.getElementById('group-filter');
let booksData = [];

document.addEventListener("DOMContentLoaded", () => {
    fetchBooks();
    setupDate();
});

async function fetchBooks() {
    try {
        const res = await fetch(`${API_URL}/books`);
        if (!res.ok) throw new Error();
        booksData = await res.json();
        if (groupSelect) {
            populateGroupDropdown(booksData);
        }
        render(booksData);
    } catch (e) {
        grid.innerHTML = '<div style="text-align:center; padding:50px;">Không thể kết nối đến máy chủ.</div>';
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
    grid.innerHTML = '';
    if (!books.length) {
        grid.innerHTML = '<div style="text-align:center;">Không tìm thấy sách.</div>';
        return;
    }

    const groups = books.reduce((acc, book) => {
        const groupName = detectGroup(book.id);
        if (!acc[groupName]) acc[groupName] = [];
        acc[groupName].push(book);
        return acc;
    }, {});

    const sortedGroupNames = Object.keys(groups).sort();

    sortedGroupNames.forEach((groupName, index) => {
        const groupBooks = groups[groupName];

        const section = document.createElement('div');
        section.className = 'category-block';
        section.style.animationDelay = `${index * 0.1}s`;

        let html = `
            <div class="category-header">
                <h2 class="category-title">${groupName}</h2>
                <span class="book-count">${groupBooks.length} items</span>
            </div>
            <div class="book-grid" style="display:grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 30px;">
        `;

        groupBooks.forEach(book => {
            const isAvail = book.available > 0;
            const img = book.image || "https://via.placeholder.com/300x450/302b63/ffffff?text=DNU";

            html += `
                <div class="book-card">
                    <img src="${img}" class="book-img">
                    <div class="book-meta">
                        <div style="font-weight:bold;">${book.title}</div>
                        <div style="font-size:0.8rem; opacity:0.8;">${book.author}</div>
                    </div>
                    <div class="book-overlay">
                        <h3 style="font-family:'Playfair Display'; color:#ffeaa7;">${book.title}</h3>
                        <p>ID: ${book.id}</p>
                        <p>SL: ${isAvail ? book.available : 0}</p>
                        <button class="btn-borrow" ${!isAvail ? 'disabled' : ''} onclick="openModal('${book.id}', '${book.title}')">
                            ${isAvail ? 'Mượn Ngay' : 'Đã Hết'}
                        </button>
                    </div>
                </div>
            `;
        });

        html += `</div>`;
        section.innerHTML = html;
        grid.appendChild(section);
    });
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
        render(booksData);
    } else {
        const filtered = booksData.filter(book => detectGroup(book.id) === selectedGroup);
        render(filtered);
    }
};

window.filterBooks = () => {
    const term = document.getElementById('search-input').value.toLowerCase().trim();
    const filtered = booksData.filter(b => b.title.toLowerCase().includes(term));
    render(filtered);
    if (groupSelect) groupSelect.value = 'all';
};

window.openModal = (id, title) => {
    document.getElementById('book-id').value = id;
    document.getElementById('book-highlight').innerHTML = `<p style="color:#00d2ff; text-align:center; margin-bottom:20px;">Sách chọn: <strong>${title}</strong></p>`;
    modal.style.display = 'block';
};

closeBtn.onclick = () => modal.style.display = 'none';
window.onclick = (e) => { if (e.target == modal) modal.style.display = 'none'; }

function setupDate() {
    const dt = new Date();
    dt.setDate(dt.getDate() + 1);
    document.getElementById('return-date').min = dt.toISOString().split('T')[0];
}

form.addEventListener('submit', async (e) => {
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
            modal.style.display = 'none';
            form.reset();
            fetchBooks();
        } else {
            alert("Lỗi: " + await res.text());
        }
    } catch (err) {
        alert("Lỗi kết nối.");
    } finally {
        btn.innerText = oldText;
        btn.disabled = false;
    }
});