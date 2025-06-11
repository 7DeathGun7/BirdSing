// AsignacionDocenteController.cs
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]
    public class AsignacionDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsignacionDocenteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Lista()
        {
            var lista = _context.AsignacionDocentes
                .Include(a => a.Docente).ThenInclude(d => d.Usuario)
                .Include(a => a.Materia)
                .Include(a => a.Grupo)
                .ToList();

            return View(lista);
        }

        public IActionResult Registro()
        {
            ViewBag.Docentes = _context.Docentes.Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario.NombreUsuario
                }).ToList();

            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem { Value = g.IdGrado.ToString(), Text = g.Grados })
                .ToList();

            ViewBag.Grupos = new List<SelectListItem>();
            ViewBag.Materias = new List<SelectListItem>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(AsignacionDocente asignacion)
        {
            if (_context.AsignacionDocentes.Any(a => a.IdGrupo == asignacion.IdGrupo && a.IdMateria == asignacion.IdMateria))
            {
                ModelState.AddModelError("", "Esta materia ya fue asignada a este grupo y grado.");
            }

            if (ModelState.IsValid)
            {
                asignacion.Activo = true;
                _context.AsignacionDocentes.Add(asignacion);
                _context.SaveChanges();
                return RedirectToAction("Lista");
            }

            ViewBag.Docentes = _context.Docentes.Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario.NombreUsuario
                }).ToList();

            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem { Value = g.IdGrado.ToString(), Text = g.Grados })
                .ToList();

            ViewBag.Grupos = _context.Grupos
                .Where(g => g.IdGrado == asignacion.IdGrado)
                .Select(g => new SelectListItem { Value = g.IdGrupo.ToString(), Text = g.Grupos })
                .ToList();

            ViewBag.Materias = _context.Materias
                .Where(m => m.IdGrado == asignacion.IdGrado)
                .Select(m => new SelectListItem { Value = m.IdMateria.ToString(), Text = m.NombreMateria })
                .ToList();

            return View(asignacion);
        }

        [HttpGet]
        public IActionResult ObtenerGruposPorGrado(int idGrado)
        {
            var grupos = _context.Grupos
                .Where(g => g.IdGrado == idGrado && g.Activo)
                .Select(g => new SelectListItem { Value = g.IdGrupo.ToString(), Text = g.Grupos })
                .ToList();

            return Json(grupos);
        }

        [HttpGet]
        public IActionResult ObtenerMateriasPorGrado(int idGrado)
        {
            var materias = _context.Materias
                .Where(m => m.IdGrado == idGrado && m.Activo)
                .Select(m => new SelectListItem { Value = m.IdMateria.ToString(), Text = m.NombreMateria })
                .ToList();

            return Json(materias);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var asignacion = _context.AsignacionDocentes.Find(id);
            if (asignacion == null) return NotFound();

            ViewBag.Docentes = _context.Docentes.Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario.NombreUsuario
                }).ToList();

            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem { Value = g.IdGrado.ToString(), Text = g.Grados })
                .ToList();

            ViewBag.Grupos = _context.Grupos
                .Where(g => g.IdGrado == asignacion.IdGrado)
                .Select(g => new SelectListItem { Value = g.IdGrupo.ToString(), Text = g.Grupos })
                .ToList();

            ViewBag.Materias = _context.Materias
                .Where(m => m.IdGrado == asignacion.IdGrado)
                .Select(m => new SelectListItem { Value = m.IdMateria.ToString(), Text = m.NombreMateria })
                .ToList();

            return View(asignacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(AsignacionDocente asignacion)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos(asignacion); // << IMPORTANTE
                return View(asignacion);
            }

            bool yaAsignado = _context.AsignacionDocentes.Any(a =>
                a.Id != asignacion.Id &&
                a.IdGrado == asignacion.IdGrado &&
                a.IdGrupo == asignacion.IdGrupo &&
                a.IdMateria == asignacion.IdMateria &&
                a.Activo);

            if (yaAsignado)
            {
                ModelState.AddModelError("", "Esta materia ya fue asignada a este grupo y grado.");
                CargarCombos(asignacion); // << IMPORTANTE
                return View(asignacion);
            }

            try
            {
                asignacion.Activo = true;
                _context.AsignacionDocentes.Update(asignacion);
                _context.SaveChanges();
                return RedirectToAction("Lista");
            }
            catch
            {
                ModelState.AddModelError("", "Error al actualizar la asignación.");
                CargarCombos(asignacion); // << IMPORTANTE
                return View(asignacion);
            }
        }


        private void CargarCombos(AsignacionDocente asignacion)
        {
            ViewBag.Docentes = _context.Docentes
                .Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario.NombreUsuario,
                    Selected = d.IdDocente == asignacion.IdDocente
                }).ToList();

            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados,
                    Selected = g.IdGrado == asignacion.IdGrado
                }).ToList();

            ViewBag.Grupos = _context.Grupos
                .Where(g => g.IdGrado == asignacion.IdGrado && g.Activo)
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = g.Grupos,
                    Selected = g.IdGrupo == asignacion.IdGrupo
                }).ToList();

            ViewBag.Materias = _context.Materias
                .Where(m => m.IdGrado == asignacion.IdGrado && m.Activo)
                .Select(m => new SelectListItem
                {
                    Value = m.IdMateria.ToString(),
                    Text = m.NombreMateria,
                    Selected = m.IdMateria == asignacion.IdMateria
                }).ToList();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var asignacion = _context.AsignacionDocentes.Find(id);
            if (asignacion != null)
            {
                _context.AsignacionDocentes.Remove(asignacion);
                _context.SaveChanges();
            }
            return RedirectToAction("Lista");
        }
    }
}
