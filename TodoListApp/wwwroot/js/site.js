const menuButton = document.getElementById('menuButton');
const sidebarContainer = document.getElementById('sidebar-container');
const sidebar = document.getElementById('sidebar');
let menuOpen = false;

menuButton.addEventListener('click', function (event) {
    menuButton.classList.toggle("change");

    if (!menuOpen) {
        sidebarContainer.style.width = "450px";
        sidebar.style.display = "block";
        menuOpen = true;
    } else {
        sidebarContainer.style.width = "75px";
        sidebar.style.display = "none";
        menuOpen = false;
    }
});