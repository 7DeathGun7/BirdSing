using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var materias = _context.Materias
                .Include(m => m.Grado)
                .ToList();
            return View(materias);
        }

        // GET: /Materias/RegistroMateria
        public IActionResult RegistroMateria()
        {
            CargarGradosEnViewBag();
            return View(new Materia());
        }

        // POST: /Materias/RegistroMateria
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegistroMateria(Materia model)
        {
            if (ModelState.IsValid)
            {
                _context.Materias.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(ListaMaterias));
            }

            CargarGradosEnViewBag();
            return View(model);
        }

        // GET: /Materias/ActualizarMateria/5
        public IActionResult ActualizarMateria(int id)
        {
            var materia = _context.Materias
                .Include(m => m.Grado)
                .FirstOrDefault(m => m.IdMateria == id);
            if (materia == null)
                return NotFound();

            CargarGradosEnViewBag();
            return View(materia);
        }

        // POST: /Materias/ActualizarMateria
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ActualizarMateria(Materia model)
        {
            if (ModelState.IsValid)
            {
                _context.Materias.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(ListaMaterias));
            }

            CargarGradosEnViewBag();
            return View(model);
        }

        // GET: /Materias/EliminarMateria/5
        [HttpPost]
        public async Task<IActionResult> EliminarMateria(int id)
        {
            // 1) Buscar la materia
            var materia = _context.Materias.Find(id);
            if (materia == null)
                return NotFound();

            // 2) Inactivar dependencias sin eliminarlas físicamente

            // 2a) MateriasDocentes
            var md = _context.MateriasDocentes
                .Where(x => x.IdMateria == id);
            foreach (var item in md)
                item.Activo = false;

            // 2b) GrupoMaterias
            var gm = _context.GrupoMaterias
                .Where(x => x.IdMateria == id);
            foreach (var item in gm)
                item.Activo = false;

            // 2c) Avisos
            var avisos = _context.Avisos
                .Where(a => a.IdMateria == id);
            foreach (var aviso in avisos)
                aviso.Activo = false;

            // 3) Inactivar materia
            materia.Activo = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListaMaterias));
        }

        // Helper privado para poblar el dropdown de Grados
        private void CargarGradosEnViewBag()
        {
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados
                .Select(g => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .ToList();
        }
    }
}
