using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class MateriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MateriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaMaterias()
        {
            var materias = _context.Materias.Include(m => m.Grado).ToList();
            return View(materias);
        }

        public IActionResult RegistroMateria()
        {
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(new Materia());
        }

        [HttpPost]
        public IActionResult RegistroMateria(Materia model)
        {
            if (ModelState.IsValid)
            {
                _context.Materias.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaMaterias");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarMateria(int id)
        {
            var materia = _context.Materias.Include(m => m.Grado).FirstOrDefault(m => m.IdMateria == id);
            if (materia == null)
            {
                return NotFound();
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(materia);
        }

        [HttpPost]
        public IActionResult ActualizarMateria(Materia model)
        {
            if (ModelState.IsValid)
            {
                _context.Materias.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaMaterias");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(model);
        }

        public IActionResult EliminarMateria(int id)
        {
            var materia = _context.Materias.Find(id);
            if (materia != null)
            {
                _context.Materias.Remove(materia);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaMaterias");
        }
    }
}