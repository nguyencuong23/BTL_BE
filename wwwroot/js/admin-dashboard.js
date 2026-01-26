// Admin Dashboard Charts
// Loan Status Donut Chart
const loanCtx = document.getElementById("loanStatusChart");
if (loanCtx) {
  new Chart(loanCtx, {
    type: "doughnut",
    data: {
      labels: ["Đã trả", "Đang mượn", "Quá hạn"],
      datasets: [
        {
          data: [
            window.adminData.totalReturned,
            window.adminData.totalBorrowing,
            window.adminData.totalOverdue,
          ],
          backgroundColor: ["#28a745", "#ffc107", "#dc3545"],
          borderWidth: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
      },
    },
  });
}

// Categories Chart
const catCtx = document.getElementById("categoriesChart");
if (catCtx) {
  new Chart(catCtx, {
    type: "doughnut",
    data: {
      labels: window.adminData.categoryLabels || [],
      datasets: [
        {
          data: window.adminData.categoryData || [],
          backgroundColor: [
            "#2563eb",
            "#16a34a",
            "#f59e0b",
            "#dc2626",
            "#8b5cf6",
            "#ec4899",
          ],
          borderWidth: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: true, position: "bottom" },
      },
    },
  });
}
