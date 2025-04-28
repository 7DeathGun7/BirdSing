using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]  // Solo Admin
    public class GrupoMateriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GrupoMateriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GrupoMaterias
        public IActionResult ListaGrupoMaterias()
        {
            var lista = _context.GrupoMaterias
                .Include(gm => gm.Grupo).ThenInclude(g => g.Grado)
                .Include(gm => gm.Materia).ThenInclude(m => m.Grado)
                .ToList();
            return View(lista);
        }

        // GET: GrupoMaterias/Create
        public IActionResult RegistroGrupoMateria()
        {
            CargarDropdowns();
            return View(new GrupoMateria());
        }

        // POST: GrupoMaterias/Create
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegistroGrupoMateria(GrupoMateria model)
        {
            if (ModelState.IsValid)
            {
                _context.GrupoMaterias.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(ListaGrupoMaterias));
            }
            CargarDropdowns();
            return View(model);
        }

        // GET: GrupoMaterias/Edit?IdGrupo=1&IdMateria=2
        public IActionResult ActualizarGrupoMateria(int idGrupo, int idMateria)
        {
            var entidad = _context.GrupoMaterias
                .FirstOrDefault(x => x.IdGrupo == idGrupo && x.IdMateria == idMateria);
            if (entidad == null) return NotFound();
            CargarDropdowns();
            return View(entidad);
        }

        // POST: GrupoMaterias/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ActualizarGrupoMateria(GrupoMateria model)
        {
            if (ModelState.IsValid)
            {
                // Necesitamos eliminar la PK antigua y volver a añadir
                var existente = _context.GrupoMaterias.Find(model.IdGrupo, model.IdMateria);
                if (existente != null)
                {
                    _context.GrupoMaterias.Remove(existente);
                    _context.GrupoMaterias.Add(model);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(ListaGrupoMaterias));
                }
            }
            CargarDropdowns();
            return View(model);
        }

        // GET: GrupoMaterias/Delete?IdGrupo=1&IdMateria=2
        public IActionResult EliminarGrupoMateria(int idGrupo, int idMateria)
        {
            var entidad = _context.GrupoMaterias.Find(idGrupo, idMateria);
            if (entidad != null)
            {
                _context.GrupoMaterias.Remove(entidad);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaGrupoMaterias));
        }

        private void CargarDropdowns()
        {
            ViewBag.Grupos = _context.Grupos
                .Include(g => g.Grado)
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = $"{g.Grado!.Grados} – {g.Grupos}"
                })
                .ToList();

            ViewBag.Materias = _context.Materias
                .Include(m => m.Grado)
                .Select(m => new SelectListItem
                {
                    Value = m.IdMateria.ToString(),
                    Text = $"{m.Grado!.Grados} – {m.NombreMateria}"
                })
                .ToList();
        }
    }
}
