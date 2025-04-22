using BirdSing.Models.ModelosViews;
using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage;
using BCrypt.Net;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaDocentes()
        {
            var docentes = _context.Docentes.Include(d => d.Usuario).ThenInclude(u => u.Rol).ToList();
            return View(docentes);
        }

        public IActionResult RegistroDocente()
        {
            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();
            return View(new UsuarioDocenteViewModel());
        }

        [HttpPost]
        public IActionResult RegistroDocente(UsuarioDocenteViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        model.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.Usuario.Password);
                        _context.Usuarios.Add(model.Usuario);
                        _context.SaveChanges();

                        model.Docente.IdUsuario = model.Usuario.IdUsuario;
                        _context.Docentes.Add(model.Docente);
                        _context.SaveChanges();

                        transaction.Commit();
                        return RedirectToAction("ListaDocentes");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Hubo un error al registrar el Docente y Usuario: " + ex.Message);
                    }
                }
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarDocente(int id)
        {
            var docente = _context.Docentes.Include(d => d.Usuario).ThenInclude(u => u.Rol).FirstOrDefault(d => d.IdDocente == id);
            if (docente == null)
            {
                return NotFound();
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();

            var model = new UsuarioDocenteViewModel
            {
                Usuario = docente.Usuario,
                Docente = docente
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ActualizarDocente(UsuarioDocenteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var docenteExistente = _context.Docentes.Find(model.Docente.IdDocente);
                var usuarioExistente = _context.Usuarios.Find(model.Usuario.IdUsuario);

                if (docenteExistente == null || usuarioExistente == null) return NotFound();

                usuarioExistente.NombreUsuario = model.Usuario.NombreUsuario;
                usuarioExistente.ApellidoPaterno = model.Usuario.ApellidoPaterno;
                usuarioExistente.ApellidoMaterno = model.Usuario.ApellidoMaterno;
                usuarioExistente.Email = model.Usuario.Email;
                usuarioExistente.IdRol = model.Usuario.IdRol;

                docenteExistente.MatriculaSEP = model.Docente.MatriculaSEP;

                _context.SaveChanges();
                return RedirectToAction("ListaDocentes");
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();

            return View(model);
        }

        public IActionResult EliminarDocente(int id)
        {
            var docente = _context.Docentes.Include(d => d.Usuario).FirstOrDefault(d => d.IdDocente == id);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
                _context.Usuarios.Remove(docente.Usuario);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaDocentes");
        }
    }
}