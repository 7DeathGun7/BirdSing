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
    [Authorize(Roles = "1")] // Sólo administradores
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DocentesController(ApplicationDbContext context)
            => _context = context;

        // GET: /Docentes/ListaDocentes
        public IActionResult ListaDocentes()
        {
            var docentes = _context.Docentes
                .Include(d => d.Usuario)
                    .ThenInclude(u => u.Rol)
                .ToList();
            return View(docentes);
        }

        // GET: /Docentes/RegistroDocente
        [HttpGet]
        public IActionResult RegistroDocente()
        {
            CargarSoloRolDocente();
            return View(new UsuarioDocenteViewModel());
        }

        // POST: /Docentes/RegistroDocente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistroDocente(UsuarioDocenteViewModel model)
        {
            if (ModelState.IsValid)
            {
                using var tx = _context.Database.BeginTransaction();
                try
                {
                    // 1) Hash y guardar Usuario
                    model.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.Usuario.Password);
                    _context.Usuarios.Add(model.Usuario);
                    _context.SaveChanges();

                    // 2) Vincular y guardar Docente
                    model.Docente.IdUsuario = model.Usuario.IdUsuario;
                    _context.Docentes.Add(model.Docente);
                    _context.SaveChanges();

                    tx.Commit();
                    return RedirectToAction(nameof(ListaDocentes));
                }
                catch (System.Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError("", "Error al registrar Docente: " + ex.Message);
                }
            }

            CargarSoloRolDocente();
            return View(model);
        }

        // GET: /Docentes/ActualizarDocente/5
        [HttpGet]
        public IActionResult ActualizarDocente(int id)
        {
            var docente = _context.Docentes
                .Include(d => d.Usuario)
                    .ThenInclude(u => u.Rol)
                .FirstOrDefault(d => d.IdDocente == id);

            if (docente == null)
                return NotFound();   // devolverá 404 si no existe el id

            var vm = new UsuarioDocenteViewModel
            {
                Usuario = docente.Usuario,
                Docente = docente
            };

            CargarSoloRolDocente();
            return View(vm);
        }

        // POST: /Docentes/ActualizarDocente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDocente(UsuarioDocenteViewModel model)
        {
            // No incluimos contraseña en el formulario de edición:
            ModelState.Remove(nameof(model.Usuario) + "." + nameof(model.Usuario.Password));

            if (ModelState.IsValid)
            {
                var usuarioExistente = _context.Usuarios.Find(model.Usuario.IdUsuario);
                var docenteExistente = _context.Docentes.Find(model.Docente.IdDocente);

                if (usuarioExistente == null || docenteExistente == null)
                    return NotFound();

                // Actualiza campos de Usuario
                usuarioExistente.NombreUsuario = model.Usuario.NombreUsuario;
                usuarioExistente.ApellidoPaterno = model.Usuario.ApellidoPaterno;
                usuarioExistente.ApellidoMaterno = model.Usuario.ApellidoMaterno;
                usuarioExistente.Email = model.Usuario.Email;
                usuarioExistente.IdRol = model.Usuario.IdRol;

                // Actualiza campos de Docente
                docenteExistente.MatriculaSEP = model.Docente.MatriculaSEP;

                _context.SaveChanges();
                return RedirectToAction(nameof(ListaDocentes));
            }

            // Si hay errores, recarga dropdown y vuelve a la vista
            CargarSoloRolDocente();
            return View(model);
        }

        // GET: /Docentes/EliminarDocente/5
        public IActionResult EliminarDocente(int id)
        {
            var docente = _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefault(d => d.IdDocente == id);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
                _context.Usuarios.Remove(docente.Usuario);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaDocentes));
        }

        // Helper: carga únicamente el rol "Docente" en ViewBag.Roles
        private void CargarSoloRolDocente()
        {
            ViewBag.Roles = _context.Roles
                .Where(r => r.Nombre == "Docente")
                .Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Nombre
                })
                .ToList();
        }
    }
}
