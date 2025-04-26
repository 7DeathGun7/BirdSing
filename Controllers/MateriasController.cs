using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo administradores
    public class MateriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MateriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Materias/ListaMaterias
        public IActionResult ListaMaterias()
        {
            // Incluimos el Grado para poder mostrar su nombre en la vista
            var materias = _context.Materias
                .Include(m => m.Grado)
                .ToList();
            return View(materias);
        }

        // GET: /Materias/RegistroMateria
        [HttpGet]
        public IActionResult RegistroMateria()
        {
            CargarGradosEnViewBag();
            return View(new Materia());
        }

        // POST: /Materias/RegistroMateria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistroMateria(Materia model)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(model);
            }

            _context.Materias.Add(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMaterias));
        }

        // GET: /Materias/ActualizarMateria/5
        [HttpGet]
        public IActionResult ActualizarMateria(int id)
        {
            var materia = _context.Materias.Find(id);
            if (materia == null) return NotFound();

            CargarGradosEnViewBag();
            return View(materia);
        }

        // POST: /Materias/ActualizarMateria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarMateria(Materia model)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(model);
            }

            _context.Materias.Update(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMaterias));
        }

        // GET: /Materias/EliminarMateria/5
        public IActionResult EliminarMateria(int id)
        {
            var materia = _context.Materias.Find(id);
            if (materia != null)
            {
                _context.Materias.Remove(materia);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaMaterias));
        }

        // Helper privado para poblar el dropdown de grados
        private void CargarGradosEnViewBag()
        {
            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .ToList();
        }
    }
}
