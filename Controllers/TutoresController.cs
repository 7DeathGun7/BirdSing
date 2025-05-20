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
        public IActionResult RegistroTutor(UsuarioTutorViewModel model)
        {
            if (ModelState.IsValid)
            {
                using IDbContextTransaction tx = _context.Database.BeginTransaction();
                try
                {
                    // 1) Hashear y guardar el usuario
                    model.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.Usuario.Password);
                    _context.Usuarios.Add(model.Usuario);
                    _context.SaveChanges();

                    // 2) Crear tutor vinculado al usuario recién creado
                    model.Tutor.IdUsuario = model.Usuario.IdUsuario;
                    _context.Tutores.Add(model.Tutor);
                    _context.SaveChanges();

                    tx.Commit();
                    return RedirectToAction(nameof(ListaTutores));
                }
                catch (System.Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError(string.Empty, "Error al registrar Tutor: " + ex.Message);
                }
            }

            // Si algo falla, recargamos el dropdown y volvemos a la vista
            CargarSoloRolTutor();
            return View(model);
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
            // La contraseña no se edita aquí, quitamos su validación
            ModelState.Remove(nameof(model.Usuario) + "." + nameof(model.Usuario.Password));

            if (ModelState.IsValid)
            {
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

                _context.SaveChanges();
                return RedirectToAction(nameof(ListaTutores));
            }

            // Si hay errores, recargamos el dropdown y regresamos a la vista
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
                tutor.Usuario!.Activo = false;

                var avisos = _context.Avisos.Where(a => a.IdTutor == tutor.IdTutor);
                foreach (var aviso in avisos)
                    aviso.Activo = false;

                await _context.SaveChangesAsync(); // ❗ le faltaba `await` en tu captura
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

