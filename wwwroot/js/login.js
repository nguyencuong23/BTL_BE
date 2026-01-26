// Toggle between login and register pages
function togglePage() {
    document.getElementById("theBook").classList.toggle("show-register");
}

// Fix button click issue: .page-front blocking clicks due to transform-style preserve-3d
document.addEventListener("DOMContentLoaded", function () {
    const pageFront = document.querySelector(".page-front");
    if (pageFront) {
        // Make cursor pointer when over button area
        pageFront.addEventListener("mousemove", function (e) {
            const btn = this.querySelector(".btn-login");
            if (btn) {
                const rect = btn.getBoundingClientRect();
                if (
                    e.clientX >= rect.left &&
                    e.clientX <= rect.right &&
                    e.clientY >= rect.top &&
                    e.clientY <= rect.bottom
                ) {
                    this.style.cursor = "pointer";
                } else {
                    this.style.cursor = "";
                }
            }
        });

        // Forward clicks to button
        pageFront.addEventListener("click", function (e) {
            const btn = this.querySelector(".btn-login");
            if (btn && e.target !== btn) {
                const rect = btn.getBoundingClientRect();
                if (
                    e.clientX >= rect.left &&
                    e.clientX <= rect.right &&
                    e.clientY >= rect.top &&
                    e.clientY <= rect.bottom
                ) {
                    btn.click();
                }
            }
        });
    }
});