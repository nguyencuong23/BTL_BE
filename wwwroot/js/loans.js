document.addEventListener('DOMContentLoaded', function () {
    // 1. XỬ LÝ CACHE 24H CHO MODAL
    const modal = document.getElementById('instruction-modal');
    const CACHE_KEY = 'hide_instruction_timestamp';
    const DAY_IN_MS = 24 * 60 * 60 * 1000;

    const savedTime = localStorage.getItem(CACHE_KEY);
    const now = new Date().getTime();

    if (modal && (!savedTime || (now - savedTime > DAY_IN_MS))) {
        setTimeout(() => {
            modal.classList.add('active');
        }, 500);
    }

    // 2. CHẠY SỐ NHẢY
    animateCounters();

    // Đóng modal khi click ra ngoài
    window.onclick = function (event) {
        if (event.target == modal) closeInstructionModal();
    }
});

function closeInstructionModal() {
    const modal = document.getElementById('instruction-modal');
    const checkbox = document.getElementById('dontShowAgain');

    if (modal) {
        if (checkbox && checkbox.checked) {
            localStorage.setItem('hide_instruction_timestamp', new Date().getTime());
        }
        modal.classList.remove('active');
    }
}

function animateCounters() {
    const counters = document.querySelectorAll('.glass-panel h2');
    counters.forEach(counter => {
        const target = +counter.innerText;
        let count = 0;
        const inc = target / 100;

        const update = () => {
            if (count < target) {
                count += inc;
                counter.innerText = Math.ceil(count);
                setTimeout(update, 20);
            } else {
                counter.innerText = target;
            }
        };
        update();
    });
}

function filterLoans() {
    const input = document.getElementById("loanSearchInput").value.toUpperCase();
    const rows = document.querySelectorAll("#loanTableBody tr");
    let hasResult = false;

    rows.forEach(row => {
        const text = row.querySelector(".book-title").innerText + row.querySelector(".book-author").innerText;
        if (text.toUpperCase().includes(input)) {
            row.style.display = "";
            hasResult = true;
        } else {
            row.style.display = "none";
        }
    });
    document.getElementById("noResultsMsg").style.display = hasResult ? "none" : "block";
}

function clearSearch() {
    document.getElementById("loanSearchInput").value = "";
    filterLoans();
}