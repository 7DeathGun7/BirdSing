using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class MateriasDocentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MateriasDocentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaMateriasDocentes()
        {
            var materiasDocentes = _context.MateriasDocentes.Include(md => md.Docente).Include(md => md.Materia).ToList();
            return View(materiasDocentes);
        }

        public IActionResult RegistroMateriaDocente()
        {
            var docentes = _context.Docentes.ToList();
            ViewBag.Docentes = docentes.Select(d => new SelectListItem
            {
                Value = d.IdDocente.ToString(),
                Text = d.Usuario.NombreUsuario
            }).ToList();
            var materias = _context.Materias.ToList();
            ViewBag.Materias = materias.Select(m => new SelectListItem
            {
                Value = m.IdMateria.ToString(),
                Text = m.NombreMateria
            }).ToList();
            return View(new MateriaDocente());
        }

        [HttpPost]
        public IActionResult RegistroMateriaDocente(MateriaDocente model)
        {
            if (ModelState.IsValid)
            {
                _context.MateriasDocentes.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaMateriasDocentes");
            }
            var docentes = _context.Docentes.ToList();
            ViewBag.Docentes = docentes.Select(d => new SelectListItem
            {
                Value = d.IdDocente.ToString(),
                Text = d.Usuario.NombreUsuario
            }).ToList();
            var materias = _context.Materias.ToList();
            ViewBag.Materias = materias.Select(m => new SelectListItem
            {
                Value = m.IdMateria.ToString(),
                Text = m.NombreMateria
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarMateriaDocente(int id)
        {
            var materiaDocente = _context.MateriasDocentes.Include(md => md.Docente).Include(md => md.Materia).FirstOrDefault(md => md.Id == id);
            if (materiaDocente == null)
            {
                return NotFound();
            }
            var docentes = _context.Docentes.ToList();
            ViewBag.Docentes = docentes.Select(d => new SelectListItem
            {
                Value = d.IdDocente.ToString(),
                Text = d.Usuario.NombreUsuario
            }).ToList();
            var materias = _context.Materias.ToList();
            ViewBag.Materias = materias.Select(m => new SelectListItem
            {
                Value = m.IdMateria.ToString(),
                Text = m.NombreMateria
            }).ToList();
            return View(materiaDocente);
        }

        [HttpPost]
        public IActionResult ActualizarMateriaDocente(MateriaDocente model)
        {
            if (ModelState.IsValid)
            {
                _context.MateriasDocentes.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaMateriasDocentes");
            }
            var docentes = _context.Docentes.ToList();
            ViewBag.Docentes = docentes.Select(d => new SelectListItem
            {
                Value = d.IdDocente.ToString(),
                Text = d.Usuario.NombreUsuario
            }).ToList();
            var materias = _context.Materias.ToList();
            ViewBag.Materias = materias.Select(m => new SelectListItem
            {
                Value = m.IdMateria.ToString(),
                Text = m.NombreMateria
            }).ToList();
            return View(model);
        }

        public IActionResult EliminarMateriaDocente(int id)
        {
            var materiaDocente = _context.MateriasDocentes.Find(id);
            if (materiaDocente != null)
            {
                _context.MateriasDocentes.Remove(materiaDocente);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaMateriasDocentes");
        }
    }
}