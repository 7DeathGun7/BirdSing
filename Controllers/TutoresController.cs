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
    public class TutoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaTutores()
        {
            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                .ThenInclude(u => u.Rol)
                .ToList();
            return View(tutores);
        }

        public IActionResult RegistroTutor()
        {
            var rolTutor = _context.Roles.FirstOrDefault(r => r.Nombre == "Tutor"); // Puedes usar ID si lo prefieres
            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = rolTutor.IdRol.ToString(),
                    Text = rolTutor.Nombre
                }
            };

            return View(new UsuarioTutorViewModel());
        }

        [HttpPost]
        public IActionResult RegistroTutor(UsuarioTutorViewModel model)
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

                        model.Tutor.IdUsuario = model.Usuario.IdUsuario;
                        _context.Tutores.Add(model.Tutor);
                        _context.SaveChanges();

                        transaction.Commit();
                        return RedirectToAction("ListaTutores");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Hubo un error al registrar el Tutor y Usuario: " + ex.Message);
                    }
                }
            }

            var rolTutor = _context.Roles.FirstOrDefault(r => r.Nombre == "Tutor");
            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = rolTutor.IdRol.ToString(),
                    Text = rolTutor.Nombre
                }
            };

            return View(model);
        }

        public IActionResult ActualizarTutor(int id)
        {
            var tutor = _context.Tutores.Include(t => t.Usuario).ThenInclude(u => u.Rol).FirstOrDefault(t => t.IdTutor == id);
            if (tutor == null)
            {
                return NotFound();
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();

            var model = new UsuarioTutorViewModel
            {
                Usuario = tutor.Usuario,
                Tutor = tutor
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ActualizarTutor(UsuarioTutorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tutorExistente = _context.Tutores.Find(model.Tutor.IdTutor);
                var usuarioExistente = _context.Usuarios.Find(model.Usuario.IdUsuario);

                if (tutorExistente == null || usuarioExistente == null) return NotFound();

                usuarioExistente.NombreUsuario = model.Usuario.NombreUsuario;
                usuarioExistente.ApellidoPaterno = model.Usuario.ApellidoPaterno;
                usuarioExistente.ApellidoMaterno = model.Usuario.ApellidoMaterno;
                usuarioExistente.Email = model.Usuario.Email;
                usuarioExistente.IdRol = model.Usuario.IdRol;

                tutorExistente.Telefono = model.Tutor.Telefono;
                tutorExistente.Direccion = model.Tutor.Direccion;

                _context.SaveChanges();
                return RedirectToAction("ListaTutores");
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();

            return View(model);
        }

        public IActionResult EliminarTutor(int id)
        {
            var tutor = _context.Tutores.Include(t => t.Usuario).FirstOrDefault(t => t.IdTutor == id);
            if (tutor != null)
            {
                _context.Tutores.Remove(tutor);
                _context.Usuarios.Remove(tutor.Usuario);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaTutores");
        }
    }
}
