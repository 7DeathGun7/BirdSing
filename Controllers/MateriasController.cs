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
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(model);
            }

            // Validación: no permitir nombre repetido en el mismo grado
            bool existe = _context.Materias.Any(m =>
                m.NombreMateria == model.NombreMateria &&
                m.IdGrado == model.IdGrado &&
                m.Activo);

            if (existe)
            {
                ModelState.AddModelError("NombreMateria", "Esta materia ya existe para el grado seleccionado.");
                CargarGradosEnViewBag();
                return View(model);
            }

            _context.Materias.Add(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMaterias));
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
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(model);
            }

            //  Validación: no permitir duplicados al actualizar
            bool duplicado = _context.Materias.Any(m =>
                m.IdMateria != model.IdMateria &&
                m.NombreMateria == model.NombreMateria &&
                m.IdGrado == model.IdGrado &&
                m.Activo);

            if (duplicado)
            {
                ModelState.AddModelError("NombreMateria", "Ya existe otra materia con ese nombre para el grado seleccionado.");
                CargarGradosEnViewBag();
                return View(model);
            }

            _context.Materias.Update(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMaterias));
        }

        // GET: /Materias/EliminarMateria/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMateria(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();

            materia.Activo = false;

            // Elimina asociaciones
            var md = _context.MateriasDocentes.Where(x => x.IdMateria == id);
            var gm = _context.GrupoMaterias.Where(x => x.IdMateria == id);
            var avisos = _context.Avisos.Where(a => a.IdMateria == id);

            foreach (var aviso in avisos) aviso.Activo = false;

            _context.MateriasDocentes.RemoveRange(md);
            _context.GrupoMaterias.RemoveRange(gm);

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
