function handleSearch(e) {
    e.preventDefault();

    const keyword = document.getElementById('searchInput').value.trim();
    const resultContainer = document.getElementById('resultContainer');
    const loadingState = document.getElementById('loadingState');
    const emptyState = document.getElementById('emptyState');
    const tableBody = document.getElementById('tableBody');
    const tableWrapper = document.getElementById('tableWrapper');

    resultContainer.style.display = 'block';
    loadingState.style.display = 'block';
    emptyState.style.display = 'none';
    tableWrapper.style.display = 'none';

    setTimeout(() => {
        const mockData = getMockData(keyword);

        loadingState.style.display = 'none';

        if (mockData.length === 0) {
            emptyState.style.display = 'block';
        } else {
            tableWrapper.style.display = 'block';
            renderTable(mockData, tableBody);
        }
    }, 1000);

    return false;
}

function renderTable(data, tbody) {
    tbody.innerHTML = '';

    data.forEach((sv, index) => {
        const statusHtml = sv.active
            ? `<span class="status-badge status-active">Đang hoạt động</span>`
            : `<span class="status-badge status-locked">Đã bị khóa</span>`;

        const fineHtml = sv.fine > 0
            ? `<span class="fine-tag">${sv.fine.toLocaleString('vi-VN')} đ</span>`
            : `<span class="fine-ok"><i class="fa-solid fa-check"></i> Không nợ</span>`;

        const avatarChar = sv.name.charAt(0).toUpperCase();

        const tr = document.createElement('tr');
        tr.className = 'table-row-anim';
        tr.style.animationDelay = `${index * 0.1}s`;

        tr.innerHTML = `
            <td><span style="font-weight: 700; color: var(--dn-blue);">${sv.id}</span></td>
            <td>
                <div class="student-info-flex">
                    <div class="student-avatar">${avatarChar}</div>
                    <div>
                        <div style="font-weight: 600;">${sv.name}</div>
                        <div style="font-size: 0.85rem; color: #888;">Khóa K16</div>
                    </div>
                </div>
            </td>
            <td>${sv.email}</td>
            <td>${sv.phone}</td>
            <td class="text-center">${statusHtml}</td>
            <td class="text-end">${fineHtml}</td>
            <td class="text-center">
                <button class="btn btn-sm btn-outline-primary rounded-circle" title="Chi tiết">
                    <i class="fa-solid fa-arrow-right"></i>
                </button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function getMockData(keyword) {
    const allStudents = [
        { id: "SV0001", name: "Nguyễn Văn An", email: "sv0001@dainam.edu.vn", phone: "0900000001", active: true, fine: 0 },
        { id: "SV0002", name: "Trần Thị Bích", email: "sv0002@dainam.edu.vn", phone: "0900000002", active: true, fine: 80000 },
        { id: "SV0003", name: "Lê Hoàng Cường", email: "sv0003@dainam.edu.vn", phone: "0900000003", active: true, fine: 0 },
        { id: "SV0004", name: "Phạm Minh Duy", email: "sv0004@dainam.edu.vn", phone: "0900000004", active: false, fine: 45000 },
        { id: "SV0005", name: "Hoàng Thị Em", email: "sv0005@dainam.edu.vn", phone: "0900000005", active: true, fine: 12000 },
    ];

    if (!keyword) return allStudents;

    const k = keyword.toLowerCase();
    return allStudents.filter(s =>
        s.id.toLowerCase().includes(k) ||
        s.name.toLowerCase().includes(k) ||
        s.email.toLowerCase().includes(k)
    );
}