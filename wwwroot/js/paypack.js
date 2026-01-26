document.addEventListener('DOMContentLoaded', () => {
    const loanTableBody = document.getElementById('loanTableBody');
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const statTotal = document.getElementById('statTotal');
    const statActive = document.getElementById('statActive');
    const statOverdue = document.getElementById('statOverdue');
    const statFine = document.getElementById('statFine');

    let loansData = [];
    const mockData = [
        {
            id: 1,
            bookName: "Nhập môn ReactJS & Redux",
            bookCode: "IT-0003",
            borrowDate: "05/12/2025",
            dueDate: "19/12/2025",
            returnDate: "06/12/2025",
            status: "DaTra",
            fine: 0
        },
        {
            id: 2,
            bookName: "Tiếng Anh chuyên ngành CNTT",
            bookCode: "NN-0007",
            borrowDate: "24/09/2025",
            dueDate: "08/10/2025",
            returnDate: "02/10/2025",
            status: "DaTra",
            fine: 0
        },
        {
            id: 4,
            bookName: "Kinh tế vi mô căn bản",
            bookCode: "KT-0001",
            borrowDate: "10/11/2025",
            dueDate: "24/11/2025",
            returnDate: "03/12/2025",
            status: "DaTra",
            fine: 45000
        },
        {
            id: 6,
            bookName: "Clean Code - Mã sạch",
            bookCode: "IT-0009",
            borrowDate: "20/01/2026",
            dueDate: "03/02/2026",
            returnDate: null,
            status: "DangMuon",
            fine: 0
        },
        {
            id: 7,
            bookName: "Giải tích 1",
            bookCode: "KH-0012",
            borrowDate: "01/01/2026",
            dueDate: "15/01/2026",
            returnDate: null,
            status: "QuaHan",
            fine: 20000
        }
    ];

    async function fetchLoans() {
        try {
            loanTableBody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center" style="padding: 40px;">
                        <i class="fa-solid fa-spinner fa-spin" style="font-size: 24px; color: var(--primary-color);"></i>
                    </td>
                </tr>`;

            await new Promise(resolve => setTimeout(resolve, 800));

            loansData = mockData;
            calculateStats(loansData);
            renderTable(loansData);
        } catch (error) {
            console.error(error);
            loanTableBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Lỗi tải dữ liệu</td></tr>';
        }
    }

    function calculateStats(data) {
        const total = data.length;
        const active = data.filter(item => item.status === 'DangMuon').length;
        const overdue = data.filter(item => item.status === 'QuaHan').length;
        const totalFine = data.reduce((sum, item) => sum + item.fine, 0);

        statTotal.textContent = total;
        statActive.textContent = active;
        statOverdue.textContent = overdue;
        statFine.textContent = formatCurrency(totalFine);
    }

    function renderStatus(status) {
        if (status === 'DaTra') return '<span class="status-badge status-success"><i class="fa-solid fa-check me-1"></i> Đã trả</span>';
        if (status === 'DangMuon') return '<span class="status-badge status-warning"><i class="fa-regular fa-clock me-1"></i> Đang mượn</span>';
        if (status === 'QuaHan') return '<span class="status-badge status-danger"><i class="fa-solid fa-circle-exclamation me-1"></i> Quá hạn</span>';
        return '<span>-</span>';
    }

    function formatCurrency(amount) {
        if (!amount || amount === 0) return '0 đ';
        return amount.toLocaleString('vi-VN') + ' đ';
    }

    function renderTable(data) {
        loanTableBody.innerHTML = '';

        if (data.length === 0) {
            loanTableBody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center" style="padding: 40px; color: var(--text-light);">
                        <i class="fa-solid fa-box-open" style="font-size: 32px; margin-bottom: 10px;"></i><br>
                        Bạn chưa có lịch sử mượn sách nào.
                    </td>
                </tr>`;
            return;
        }

        data.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>
                    <div class="book-info">
                        <span class="book-name">${item.bookName}</span>
                        <span class="book-code">${item.bookCode}</span>
                    </div>
                </td>
                <td><span class="date-info">${item.borrowDate}</span></td>
                <td><span class="date-info" style="color: ${item.status === 'QuaHan' ? 'var(--danger)' : 'inherit'}">${item.dueDate}</span></td>
                <td><span class="${item.returnDate ? 'date-info' : 'date-faded'}">${item.returnDate || 'Chưa trả'}</span></td>
                <td class="text-center">${renderStatus(item.status)}</td>
                <td class="text-right">
                    <span class="${item.fine > 0 ? 'fine-amount' : ''}">${formatCurrency(item.fine)}</span>
                </td>
                <td class="text-center">
                    <button class="btn-detail" title="Xem chi tiết">
                        <i class="fa-solid fa-chevron-right"></i>
                    </button>
                </td>
            `;
            loanTableBody.appendChild(row);
        });
    }

    function filterData() {
        const searchTerm = searchInput.value.toLowerCase();
        const statusValue = statusFilter.value;

        let filteredData = loansData.filter(item => {
            const matchesSearch = item.bookName.toLowerCase().includes(searchTerm) || item.bookCode.toLowerCase().includes(searchTerm);
            const matchesStatus = statusValue === 'all' || item.status === statusValue;
            return matchesSearch && matchesStatus;
        });

        renderTable(filteredData);
    }

    searchInput.addEventListener('input', filterData);
    statusFilter.addEventListener('change', filterData);


    fetchLoans();
});