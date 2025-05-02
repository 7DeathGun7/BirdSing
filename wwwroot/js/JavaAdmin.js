function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    sidebar.classList.toggle("visible");
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


// Si tienes un botón con id="logoutButton", activa el logout manual
const logoutBtn = document.getElementById('logoutButton');
if (logoutBtn) {
    logoutBtn.addEventListener('click', function () {
        localStorage.removeItem('usuario');
        window.location.href = '../Log.html';
    });
}