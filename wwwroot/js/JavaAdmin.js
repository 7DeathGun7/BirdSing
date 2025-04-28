function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    sidebar.classList.toggle("visible");
}

// Mostrar/ocultar submenús al hacer clic en el enlace principal del dropdown
document.querySelectorAll('.dropdown > a').forEach(link => {
    link.addEventListener('click', function (e) {
        e.preventDefault(); // Previene redirección
        const submenu = this.nextElementSibling;
        if (submenu && submenu.classList.contains('dropdown-content')) {
            submenu.style.display = (submenu.style.display === 'block') ? 'none' : 'block';
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
