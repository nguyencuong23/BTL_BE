// Main initialization
// Main initialization
document.addEventListener("DOMContentLoaded", function () {
  loadDashboardData();
  loadTrendCharts();
});

// ... (keep existing loadDashboardData) ...

function loadTrendCharts() {
  fetch("/api/admin/borrowing-trends")
    .then((response) => response.json())
    .then((data) => {
      renderBorrowVolumeChart(data.borrowVolume);
      renderActiveLoansChart(data.activeLoans);
    })
    .catch((error) => console.error("Error loading trend charts:", error));
}

function renderBorrowVolumeChart(data) {
  const ctx = document.getElementById("borrowVolumeChart");
  if (!ctx) return;

  const dates = data.map((d) => {
    const date = new Date(d.date);
    return `${date.getDate()}/${date.getMonth() + 1}`;
  });
  const counts = data.map((d) => d.count);

  // Create Gradient
  const gradient = ctx.getContext("2d").createLinearGradient(0, 0, 0, 300);
  gradient.addColorStop(0, "rgba(59, 130, 246, 1)"); // #3b82f6 (blue-500)
  gradient.addColorStop(1, "rgba(147, 197, 253, 0.3)"); // #93c5fd (blue-300)

  new Chart(ctx, {
    type: "bar",
    data: {
      labels: dates,
      datasets: [
        {
          label: "Lượt mượn",
          data: counts,
          backgroundColor: gradient,
          borderRadius: 4,
          barPercentage: 0.6,
          categoryPercentage: 0.8,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          padding: 10,
          callbacks: {
            title: (context) => `Ngày: ${context[0].label}`,
            label: (context) => ` ${context.raw} lượt mượn`,
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { color: "#f3f4f6", borderDash: [2, 4] },
          ticks: { precision: 0 },
        },
        x: {
          grid: { display: false },
          ticks: { maxTicksLimit: 10 },
        },
      },
      animation: {
        duration: 2000,
        easing: "easeOutQuart",
      },
    },
  });
}

function renderActiveLoansChart(data) {
  const ctx = document.getElementById("activeLoansChart");
  if (!ctx) return;

  const dates = data.map((d) => {
    const date = new Date(d.date);
    return `${date.getDate()}/${date.getMonth() + 1}`;
  });
  const counts = data.map((d) => d.count);

  new Chart(ctx, {
    type: "line",
    data: {
      labels: dates,
      datasets: [
        {
          label: "Sách đang mượn",
          data: counts,
          borderColor: "#10b981", // emerald-500
          backgroundColor: "rgba(16, 185, 129, 0.1)",
          borderWidth: 3,
          tension: 0.4, // Smooth curve
          fill: true,
          pointBackgroundColor: "#ffffff",
          pointBorderColor: "#10b981",
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 6,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          padding: 10,
          intersect: false,
          mode: "index",
          callbacks: {
            title: (context) => `Ngày: ${context[0].label}`,
            label: (context) => ` ${context.raw} sách đang mượn`,
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { color: "#f3f4f6", borderDash: [2, 4] },
          ticks: { precision: 0 },
        },
        x: {
          grid: { display: false },
          ticks: { maxTicksLimit: 10 },
        },
      },
      interaction: {
        mode: "index",
        intersect: false,
      },
    },
  });
}

// Function to fetch and update general dashboard data
// Function to fetch and update general dashboard data
function loadDashboardData() {
  // 1. Load Charts & Tables Data (Old API)
  fetch("/api/admin/dashboard-stats")
    .then((response) => response.json())
    .then((data) => {
      // Render Charts
      if (document.getElementById("loanStatusChart"))
        renderLoanStatusChart(data);
      if (document.getElementById("categoriesChart"))
        renderCategoriesChart(data.categoryStats, "categoriesChart");

      // Render Tables
      if (document.getElementById("recent-loans-table-body"))
        renderRecentLoans(data.recentLoans);
      if (document.getElementById("top-books-list"))
        renderTopBooks(data.topBooks);
    })
    .catch((error) => console.error("Error loading charts data:", error));

  // 2. Load Summary Cards Data (New API)
  fetch("/api/admin/dashboard-summary")
    .then((response) => {
      if (!response.ok) throw new Error("Network response was not ok");
      return response.json();
    })
    .then((data) => {
      // Card 1: Books
      updateElementText("stats-total-books", data.totalBooks);
      const bookFooter = `<span class="text-white fw-bold"><i class="fas fa-swatchbook"></i> ${data.totalTitles} đầu sách</span>`;
      updateElementHtml("stats-footer-books", bookFooter);

      // Card 2: Readers
      updateElementText("stats-total-readers", data.totalStudents);
      updateElementHtml(
        "stats-footer-readers",
        `<i class="fas fa-book-reader text-white"></i> <span class="text-white fw-bold">${data.activeBorrowers} / ${data.totalStudents}</span> <span class="text-white fw-bold">đang mượn sách</span>`,
      );

      // Card 3: Borrowing
      updateElementText("stats-total-borrowing", data.totalBorrowing);
      const borrowingFooter =
        data.dueSoonCount > 0
          ? `<span class="text-white fw-bold"><i class="fas fa-clock"></i> ${data.dueSoonCount} lượt</span> <span class="text-white fw-bold">sắp đến hạn (3 ngày)</span>`
          : `<span class="text-white fw-bold"><i class="fas fa-check-circle"></i> Hôm nay: +${data.borrowedToday} lượt</span>`;
      updateElementHtml("stats-footer-borrowing", borrowingFooter);

      // Card 4: Overdue
      updateElementText("stats-total-overdue", data.totalOverdue);
      let overdueFooter = "";

      const fineFormatted = new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
      }).format(data.totalFine || 0);

      overdueFooter = `<i class="fas fa-coins text-white"></i> <span class="text-white fw-bold">Tổng tiền phạt: ${fineFormatted}</span>`;

      updateElementHtml("stats-footer-overdue", overdueFooter);
    })
    .catch((error) => {
      console.error("Error loading summary data:", error);
      // Show error in specific element if possible
    });
}
function updateElementHtml(id, html) {
  const el = document.getElementById(id);
  if (el) el.innerHTML = html;
}

// Function called explicitly by Statistics page
function loadStatisticsPage() {
  // Already loading dashboard data via DOMContentLoaded,
  // but we can add monthly stats fetching here
  fetch("/api/admin/monthly-stats")
    .then((response) => response.json())
    .then((data) => {
      if (document.getElementById("monthlyLoansChart")) {
        renderMonthlyLoansChart(data);
      }
    })
    .catch((error) => console.error("Error loading monthly stats:", error));
}

// Helper to update text safely
function updateElementText(id, text) {
  const el = document.getElementById(id);
  if (el) el.innerText = text;
}

// --- Chart Configuration & Plugins ---

// 1. Center Text Plugin
const centerTextPlugin = {
  id: "centerText",
  beforeDraw: function (chart) {
    if (chart.config.type !== "doughnut") return;

    var width = chart.width,
      height = chart.height,
      ctx = chart.ctx;

    ctx.restore();

    // Main Text (Value/Count)
    var fontSize = (height / 160).toFixed(2);
    ctx.font = "bold " + fontSize + "em sans-serif";
    ctx.textBaseline = "middle";
    ctx.fillStyle = "#333";

    var text = chart.config.options.plugins.centerText.text || "",
      textX = Math.round((width - ctx.measureText(text).width) / 2),
      textY = height / 2;

    ctx.fillText(text, textX, textY);

    // Subtitle (Label)
    if (chart.config.options.plugins.centerText.subText) {
      var subFontSize = (height / 240).toFixed(2);
      ctx.font = "normal " + subFontSize + "em sans-serif";
      ctx.fillStyle = "#666";
      var subText = chart.config.options.plugins.centerText.subText,
        subTextX = Math.round((width - ctx.measureText(subText).width) / 2),
        subTextY = height / 2 + 20;
      ctx.fillText(subText, subTextX, subTextY);
    }

    ctx.save();
  },
};

// Register the plugin
Chart.register(centerTextPlugin);

// --- Chart Rendering Functions ---

function renderLoanStatusChart(data) {
  const ctx = document.getElementById("loanStatusChart");
  if (!ctx) return;

  const totalLoans =
    data.totalReturned + data.totalBorrowing + data.totalOverdue;
  const configData = {
    labels: ["Đã trả", "Đang mượn", "Quá hạn"],
    datasets: [
      {
        data: [data.totalReturned, data.totalBorrowing, data.totalOverdue],
        backgroundColor: ["#22c55e", "#f59e0b", "#ef4444"],
        borderWidth: 2,
        borderColor: "#ffffff",
        hoverOffset: 10,
        borderRadius: 5,
      },
    ],
  };

  new Chart(ctx, {
    type: "doughnut",
    data: configData,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: "75%", // Thicker donut
      layout: {
        padding: 20,
      },
      plugins: {
        legend: { display: false }, // Hide default legend
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          padding: 12,
          callbacks: {
            label: function (context) {
              let label = context.label || "";
              let value = context.raw || 0;
              let percentage =
                totalLoans > 0 ? Math.round((value / totalLoans) * 100) : 0;
              return ` ${label}: ${value} (${percentage}%)`;
            },
          },
        },
        centerText: {
          text: totalLoans.toString(),
          subText: "Tổng phiếu",
        },
      },
      interaction: {
        mode: "nearest",
        intersect: true,
      },
    },
  });

  // Generate Custom Legend
  generateCustomLegend("loanStatusLegend", configData);
}

function renderCategoriesChart(categoryStats, canvasId) {
  const ctx = document.getElementById(canvasId);
  if (!ctx) return;

  if (!categoryStats || categoryStats.length === 0) return;

  // Map category names to explicit colors if needed, or use a palette
  const colorMap = {
    CNTT: "#3b82f6",
    "Công nghệ thông tin": "#3b82f6",
    "Khoa học": "#22c55e",
    "Kinh tế": "#f59e0b",
    "Ngoại ngữ": "#ef4444",
    "Văn học": "#8b5cf6",
  };

  // Create fallback palette
  const defaultColors = [
    "#3b82f6",
    "#22c55e",
    "#f59e0b",
    "#ef4444",
    "#8b5cf6",
    "#ec4899",
    "#6366f1",
  ];

  const labels = categoryStats.map((c) => c.label);
  const dataValues = categoryStats.map((c) => c.count);
  const total = dataValues.reduce((a, b) => a + b, 0);

  const backgroundColors = labels.map((label, index) => {
    // Try to find approximate match in keys
    const key = Object.keys(colorMap).find((k) => label.includes(k));
    return key ? colorMap[key] : defaultColors[index % defaultColors.length];
  });

  const configData = {
    labels: labels,
    datasets: [
      {
        data: dataValues,
        backgroundColor: backgroundColors,
        borderWidth: 2,
        borderColor: "#ffffff",
        hoverOffset: 10,
        borderRadius: 5,
      },
    ],
  };

  new Chart(ctx, {
    type: "doughnut",
    data: configData,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: "65%",
      layout: {
        padding: 20,
      },
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          padding: 12,
          callbacks: {
            label: function (context) {
              let label = context.label || "";
              let value = context.raw || 0;
              let percentage =
                total > 0 ? Math.round((value / total) * 100) : 0;
              return ` ${label}: ${value} (${percentage}%)`;
            },
          },
        },
        centerText: {
          text: "Thể loại",
          subText: "Sách",
        },
      },
    },
  });

  // Generate Custom Legend
  // For categories, we might want to attach it to a different ID or the same structure
  // Current HTML for categories doesn't have a distinct legend container ID in Index.cshtml yet (I need to fix HTML too)
  // Assuming I will add 'categoriesLegend' to HTML
  const legendId =
    canvasId === "categoriesChart"
      ? "categoriesLegend"
      : "detailsCategoriesLegend";
  if (document.getElementById(legendId)) {
    generateCustomLegend(legendId, configData);
  }
}

function renderMonthlyLoansChart(monthlyData) {
  const ctx = document.getElementById("monthlyLoansChart");
  if (!ctx) return;

  const labels = monthlyData.map((d) => `Tháng ${d.month}`);
  const data = monthlyData.map((d) => d.count);

  new Chart(ctx, {
    type: "bar",
    data: {
      labels: labels,
      datasets: [
        {
          label: "Số lượt mượn",
          data: data,
          backgroundColor: "#3b82f6",
          borderRadius: 4,
          hoverBackgroundColor: "#2563eb",
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          position: "nearest",
          backgroundColor: "rgba(0,0,0,0.8)",
          padding: 10,
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { color: "#f3f4f6" },
          ticks: { precision: 0 },
        },
        x: {
          grid: { display: false },
        },
      },
    },
  });
}

// --- Helper for Custom Legend ---
function generateCustomLegend(elementId, data) {
  const container = document.getElementById(elementId);
  if (!container) return;

  let html = '<div class="custom-legend-container">';
  const total = data.datasets[0].data.reduce((a, b) => a + b, 0);

  data.labels.forEach((label, index) => {
    const value = data.datasets[0].data[index];
    const color = data.datasets[0].backgroundColor[index];
    const percentage = total > 0 ? Math.round((value / total) * 100) : 0; // Use simple round

    // Only show if relevant or if user wants all (User said <5% hide label on chart, but usually legend shows all.
    // User said: "Nhãn dữ liệu... Nếu lát < 5% thì không hiển thị". This usually refers to datalabels on the chart itself.
    // For legend, standard is to show all. I will show all in legend.)

    html += `
            <div class="legend-item d-flex align-items-center justify-content-between mb-2">
                <div class="d-flex align-items-center">
                    <span class="legend-color" style="background-color: ${color}; width: 12px; height: 12px; border-radius: 3px; display: inline-block; margin-right: 8px;"></span>
                    <span class="legend-label text-muted small">${label}</span>
                </div>
                <div class="legend-stats text-end">
                    <span class="legend-value fw-bold ms-2">${value}</span>
                    <small class="legend-percent text-muted ms-1">(${percentage}%)</small>
                </div>
            </div>
        `;
  });
  html += "</div>";
  container.innerHTML = html;
}

// --- List/Table Rendering Functions ---

function renderRecentLoans(loans) {
  const tbody = document.getElementById("recent-loans-table-body");
  if (!tbody) return;

  if (!loans || loans.length === 0) {
    tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-muted">Chưa có phiếu mượn nào</td></tr>`;
    return;
  }

  tbody.innerHTML = loans
    .map((loan) => {
      let statusBadge = "";
      // Check API response structure: Status: "returned" | "overdue" | "borrowing"
      if (loan.status === "returned")
        statusBadge = '<span class="badge badge-returned">Đã trả</span>';
      else if (loan.status === "overdue")
        statusBadge = '<span class="badge badge-overdue">Quá hạn</span>';
      else statusBadge = '<span class="badge badge-borrowing">Đang mượn</span>';

      return `
            <tr>
                <td><span class="badge bg-secondary">#${loan.loanId}</span></td>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="user-avatar-small">${loan.userFullName.charAt(0)}</div>
                        <span class="ms-2">${loan.userFullName}</span>
                    </div>
                </td>
                <td><small>${loan.bookTitle}</small></td>
                <td><small>${new Date(loan.borrowDate).toLocaleDateString("vi-VN")}</small></td>
                <td><small>${new Date(loan.dueDate).toLocaleDateString("vi-VN")}</small></td>
                <td>${statusBadge}</td>
            </tr>
        `;
    })
    .join("");
}

function renderTopBooks(books) {
  const container = document.getElementById("top-books-list");
  if (!container) return;

  if (!books || books.length === 0) {
    container.innerHTML = `<div class="text-center text-muted py-4"><p>Chưa có dữ liệu</p></div>`;
    return;
  }

  container.innerHTML = books
    .map((book, index) => {
      const rankClass =
        index === 0
          ? "gold"
          : index === 1
            ? "silver"
            : index === 2
              ? "bronze"
              : "";
      return `
            <div class="top-book-item">
                <div class="rank-badge ${rankClass}">${index + 1}</div>
                <div class="book-info">
                    <div class="book-title">${book.title}</div>
                    <small class="text-muted">${book.author}</small>
                </div>
                <div class="borrow-count">
                    <span class="badge bg-primary">${book.borrowCount} <i class="fas fa-book-reader ms-1"></i></span>
                </div>
            </div>
        `;
    })
    .join("");
}
