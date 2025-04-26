function toggleSubmenu(id) {
    var submenu = document.getElementById(id);
    submenu.style.display = (submenu.style.display === "block") ? "none" : "block";
}

function loadPage(page) {
    document.getElementById("contentFrame").src = page;
}

function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    sidebar.classList.toggle("hidden");
}

// Submenús desplegables
document.querySelectorAll('.dropdown > a').forEach(link => {
    link.addEventListener('click', function (e) {
        e.preventDefault(); // Previene el salto si tiene href
        const submenu = this.nextElementSibling;
        if (submenu && submenu.classList.contains('dropdown-content')) {
            const isVisible = submenu.style.display === 'block';
            submenu.style.display = isVisible ? 'none' : 'block';
        }
    });
});

document.getElementById('logoutButton').addEventListener('click', function () {
    localStorage.removeItem('usuario');
    window.location.href = '../Log.html';
});