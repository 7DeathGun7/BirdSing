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
            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario).ToList(), "IdDocente", "Usuario.NombreUsuario");
            ViewBag.Materias = new SelectList(_context.Materias.ToList(), "IdMateria", "NombreMateria");
            ViewBag.Grupos = new SelectList(_context.Grupos.Include(g => g.Grado).ToList(), "IdGrupo", "Grupos");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(AsignacionDocente asignacion)
        {
            if (ModelState.IsValid)
            {
                _context.AsignacionDocentes.Add(asignacion);
                _context.SaveChanges();
                return RedirectToAction("Lista");
            }

            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario).ToList(), "IdDocente", "Usuario.NombreUsuario", asignacion.IdDocente);
            ViewBag.Materias = new SelectList(_context.Materias.ToList(), "IdMateria", "NombreMateria", asignacion.IdMateria);
            ViewBag.Grupos = new SelectList(_context.Grupos.Include(g => g.Grado).ToList(), "IdGrupo", "Grupos", asignacion.IdGrupo);
            return View(asignacion);
        }
    }
}
