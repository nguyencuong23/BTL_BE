function openPaymentModal() {
  const modal = document.getElementById("paymentModal");
  if (modal) {
    modal.style.display = "flex";
    document.body.style.overflow = "hidden";
  }
}

function closePaymentModal() {
  const modal = document.getElementById("paymentModal");
  if (modal) {
    modal.style.display = "none";
    document.body.style.overflow = "auto";
  }
}

// Close on click outside
document.addEventListener("DOMContentLoaded", function () {
  const modal = document.getElementById("paymentModal");
  if (modal) {
    modal.addEventListener("click", function (e) {
      if (e.target === this) {
        closePaymentModal();
      }
    });
  }
});
