document.addEventListener("DOMContentLoaded", function() {
    const btnOpen = document.getElementById("btn_open_fixed_sidebar");
    const btnClose = document.getElementById("btn_close_fixed_sidebar");
    const sidebar = document.getElementById("fixed_sidebar");
    const overlay = document.getElementById("overlay");

    function openSidebar() {
        overlay.classList.remove("opacity-0", "invisible");
        sidebar.classList.remove("-translate-x-full");
    }

    function closeSidebar() {
        overlay.classList.add("opacity-0", "invisible");
        sidebar.classList.add("-translate-x-full");
    }

    btnOpen?.addEventListener("click", openSidebar);
    btnClose?.addEventListener("click", closeSidebar);
    overlay?.addEventListener("click", closeSidebar);
});