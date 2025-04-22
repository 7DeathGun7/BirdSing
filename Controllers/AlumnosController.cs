using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaAlumnos()
        {
            var alumnos = _context.Alumnos.Include(a => a.Grado).Include(a => a.Grupo).ToList();
            return View(alumnos);
        }

        public IActionResult RegistroAlumno()
        {
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            var grupos = _context.Grupos.ToList();
            ViewBag.Grupos = grupos.Select(g => new SelectListItem
            {
                Value = g.IdGrupo.ToString(),
                Text = g.Grupos
            }).ToList();
            return View(new Alumno());
        }

        [HttpPost]
        public IActionResult RegistroAlumno(Alumno model)
        {
            if (ModelState.IsValid)
            {
                _context.Alumnos.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaAlumnos");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            var grupos = _context.Grupos.ToList();
            ViewBag.Grupos = grupos.Select(g => new SelectListItem
            {
                Value = g.IdGrupo.ToString(),
                Text = g.Grupos
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarAlumno(int id)
        {
            var alumno = _context.Alumnos.Include(a => a.Grado).Include(a => a.Grupo).FirstOrDefault(a => a.MatriculaAlumno == id);
            if (alumno == null)
            {
                return NotFound();
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            var grupos = _context.Grupos.ToList();
            ViewBag.Grupos = grupos.Select(g => new SelectListItem
            {
                Value = g.IdGrupo.ToString(),
                Text = g.Grupos
            }).ToList();
            return View(alumno);
        }

        [HttpPost]
        public IActionResult ActualizarAlumno(Alumno model)
        {
            if (ModelState.IsValid)
            {
                _context.Alumnos.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaAlumnos");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            var grupos = _context.Grupos.ToList();
            ViewBag.Grupos = grupos.Select(g => new SelectListItem
            {
                Value = g.IdGrupo.ToString(),
                Text = g.Grupos
            }).ToList();
            return View(model);
        }

        public IActionResult EliminarAlumno(int id)
        {
            var alumno = _context.Alumnos.Find(id);
            if (alumno != null)
            {
                _context.Alumnos.Remove(alumno);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaAlumnos");
        }
    }
}