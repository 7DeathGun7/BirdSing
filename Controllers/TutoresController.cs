using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using BirdSing.Models.ModelosViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BCrypt.Net;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]  // Admin solamente
    public class TutoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tutores/ListaTutores
        public IActionResult ListaTutores()
        {
            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                    .ThenInclude(u => u.Rol)
                .ToList();
            return View(tutores);
        }

        // GET: Tutores/RegistroTutor
        public IActionResult RegistroTutor()
        {
            CargarSoloRolTutor();
            return View(new UsuarioTutorViewModel());
        }

        // POST: Tutores/RegistroTutor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroTutor(UsuarioTutorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Mostrar errores
            }

            // Validación de correo (opcional)
            if (_context.Usuarios.Any(u => u.Email == model.Usuario.Email))
            {
                ModelState.AddModelError("Usuario.Email", "El correo ya está registrado.");
                return View(model);
            }

            // Hashear contraseña
            model.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.Usuario.Password);
            model.Usuario.IdRol = 3; // Forzar Rol Tutor

            _context.Usuarios.Add(model.Usuario);
            await _context.SaveChangesAsync();

            // Obtener ID del usuario insertado
            int idUsuario = model.Usuario.IdUsuario;

            model.Tutor.IdUsuario = idUsuario;
            _context.Tutores.Add(model.Tutor);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tutor ({model.Usuario.Email}) agregado correctamente.";
            return RedirectToAction("ListaTutores");

        }


        // GET: Tutores/ActualizarTutor/5
        public IActionResult ActualizarTutor(int id)
        {
            var tutor = _context.Tutores
                .Include(t => t.Usuario)
                    .ThenInclude(u => u.Rol)
                .FirstOrDefault(t => t.IdTutor == id);

            if (tutor == null)
                return NotFound();

            var vm = new UsuarioTutorViewModel
            {
                Tutor = tutor,
                Usuario = tutor.Usuario
            };

            CargarSoloRolTutor();
            return View(vm);
        }

        // POST: Tutores/ActualizarTutor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarTutor(UsuarioTutorViewModel model)
        {
            // Ignorar la validación de contraseña porque no se edita aquí
            ModelState.Remove(nameof(model.Usuario) + "." + nameof(model.Usuario.Password));

            if (ModelState.IsValid)
            {
                // Verificar si el correo ya está usado por otro usuario
                var emailDuplicado = _context.Usuarios
                    .Any(u => u.Email == model.Usuario.Email && u.IdUsuario != model.Usuario.IdUsuario);

                if (emailDuplicado)
                {
                    ModelState.AddModelError("Usuario.Email", "Este correo ya está registrado por otro tutor.");
                    CargarSoloRolTutor();
                    return View(model);
                }

                // Obtener los registros existentes
                var tutorExistente = _context.Tutores.Find(model.Tutor.IdTutor);
                var usuarioExistente = _context.Usuarios.Find(model.Usuario.IdUsuario);

                if (tutorExistente == null || usuarioExistente == null)
                    return NotFound();

                // Actualizar datos del usuario
                usuarioExistente.NombreUsuario = model.Usuario.NombreUsuario;
                usuarioExistente.ApellidoPaterno = model.Usuario.ApellidoPaterno;
                usuarioExistente.ApellidoMaterno = model.Usuario.ApellidoMaterno;
                usuarioExistente.Email = model.Usuario.Email;
                usuarioExistente.IdRol = model.Usuario.IdRol;

                // Actualizar datos del tutor
                tutorExistente.Telefono = model.Tutor.Telefono;
                tutorExistente.Direccion = model.Tutor.Direccion;
                tutorExistente.Coordenadas = model.Tutor.Coordenadas;

                _context.SaveChanges();
                return RedirectToAction(nameof(ListaTutores));
            }

            // Si hay errores, recargar dropdown de rol
            CargarSoloRolTutor();
            return View(model);
        }


        // GET: Tutores/EliminarTutor/5
        public async Task<IActionResult> EliminarTutor(int id)
        {
            var tutor = _context.Tutores
                .Include(t => t.Usuario)
                .FirstOrDefault(t => t.IdTutor == id);

            if (tutor != null)
            {
                tutor.Activo = false;
                tutor.Usuario.Activo = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ListaTutores));
        }


        // Helper: solo cargamos el rol "Tutor" en la lista desplegable
        private void CargarSoloRolTutor()
        {
            var rolTutor = _context.Roles
                .Where(r => r.Nombre == "Tutor")
                .Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Nombre
                })
                .ToList();

            ViewBag.Roles = rolTutor;
        }
    }
}

