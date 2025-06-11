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
            // Evita errores de validación si el rol viene oculto
            ModelState.Remove(nameof(model.Usuario) + "." + nameof(model.Usuario.IdRol));

            // Validar duplicación de correo
            if (_context.Usuarios.Any(u => u.Email == model.Usuario.Email))
            {
                ModelState.AddModelError("Usuario.Email", "Este correo ya está registrado.");
            }

            // Validar duplicación de matrícula
            if (_context.Docentes.Any(d => d.MatriculaSEP == model.Docente.MatriculaSEP))
                ModelState.AddModelError("Docente.MatriculaSEP", "Esta matrícula ya está registrada.");
            

            if (ModelState.IsValid)
            {
                using var tx = _context.Database.BeginTransaction();
                try
                {
                    // Asignar rol Docente
                    var rolDocente = _context.Roles.FirstOrDefault(r => r.Nombre == "Docente");
                    if (rolDocente == null)
                    {
                        ModelState.AddModelError("", "No se encontró el rol Docente.");
                        CargarSoloRolDocente();
                        return View(model);
                    }
                    model.Usuario.IdRol = rolDocente.IdRol;
                    // Guardar Usuario con contraseña hasheada
                    model.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.Usuario.Password);
                    _context.Usuarios.Add(model.Usuario);
                    _context.SaveChanges();

                    // 🔍 Verificar si se generó el Id
                    if (model.Usuario.IdUsuario == 0)
                    {
                        tx.Rollback();
                        ModelState.AddModelError("", "No se pudo generar el Id del Usuario.");
                        return View(model);
                    }

                    // Diagnóstico: revisar si se guardó el Usuario
                    Console.WriteLine("✅ Usuario guardado con Id: " + model.Usuario.IdUsuario);

                    // Guardar Docente vinculado
                    model.Docente.IdUsuario = model.Usuario.IdUsuario;
                    Console.WriteLine("➡️  Se asignará al Docente el ID de usuario: " + model.Docente.IdUsuario);
                    _context.Docentes.Add(model.Docente);
                    _context.SaveChanges();

                    Console.WriteLine("✅ Docente guardado correctamente.");


                    tx.Commit();
                    return RedirectToAction(nameof(ListaDocentes));
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError("", "Error al registrar Docente: " + ex.Message);
                }
            }

            // DEBUG: Mostrar errores del modelo en consola
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error en '{key}': {error.ErrorMessage}");
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

            bool correoDuplicado = _context.Usuarios
       .Any(u => u.Email == model.Usuario.Email && u.IdUsuario != model.Usuario.IdUsuario);
            if (correoDuplicado)
                ModelState.AddModelError("Usuario.Email", "Este correo ya está registrado por otro usuario.");

            // Validar duplicación de matrícula (por otro docente)
            bool matriculaDuplicada = _context.Docentes
                .Any(d => d.MatriculaSEP == model.Docente.MatriculaSEP && d.IdDocente != model.Docente.IdDocente);
            if (matriculaDuplicada)
                ModelState.AddModelError("Docente.MatriculaSEP", "Esta matrícula ya está registrada por otro docente.");

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
        public async Task<IActionResult> EliminarDocente(int id)
        {
            var docente = _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefault(d => d.IdDocente == id);

            if (docente != null)
            {
                docente.Activo = false;
                docente.Usuario!.Activo = false;

                var avisos = _context.Avisos.Where(a => a.IdDocente == docente.IdDocente);
                foreach (var aviso in avisos)
                    aviso.Activo = false;

                await _context.SaveChangesAsync();
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
