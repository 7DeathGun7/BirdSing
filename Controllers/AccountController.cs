using Microsoft.AspNetCore.Mvc;
using BirdSing.Models;
using BirdSing.Data;
using BCrypt.Net;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BirdSing.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == model.Email);
                if (usuario == null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
                {
                    ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                    return View(model);
                }

                bool requiereCambio = false;

                if (usuario.IdRol == 2 && BCrypt.Net.BCrypt.Verify("Docen73", usuario.Password))
                    requiereCambio = true;
                if (usuario.IdRol == 3 && BCrypt.Net.BCrypt.Verify("Tu7o4", usuario.Password))
                    requiereCambio = true;

                if (requiereCambio)
                {
                    HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
                    HttpContext.Session.SetInt32("Rol", usuario.IdRol);
                    return RedirectToAction("ForzarCambioContrasena");
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var authProps = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

                // ✅ Redirigir si existe ReturnUrl
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirección por rol
                return usuario.IdRol switch
                {
                    1 => RedirectToAction("Index", "PanelAdmin"),
                    2 => RedirectToAction("Index", "PanelDocente"),
                    3 => RedirectToAction("Index", "PanelTutor"),
                    _ => RedirectToAction("Login")
                };
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        // GET: muestra el formulario
        [HttpGet]
        public IActionResult ForzarCambioContrasena()
        {
            return View();
        }

        // POST: recibe la nueva contraseña
        [HttpPost]
        public IActionResult ForzarCambioContrasena(string nuevaContrasena)
        {
            var id = HttpContext.Session.GetInt32("UsuarioId");
            var rol = HttpContext.Session.GetInt32("Rol");

            if (id == null || rol == null)
                return RedirectToAction("Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == id && u.IdRol == rol);
            if (usuario != null)
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
                _context.SaveChanges();
                TempData["Mensaje"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Login");
            }

            return View();
        }


    }
}
