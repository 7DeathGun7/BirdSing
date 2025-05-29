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
            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario), "IdDocente", "Usuario.NombreUsuario");
            ViewBag.Grados = new SelectList(_context.Grados.ToList(), "IdGrado", "Grados"); // NUEVO
            ViewBag.Grupos = new SelectList(_context.Grupos.ToList(), "IdGrupo", "Grupos");
            ViewBag.Materias = new SelectList(_context.Materias.ToList(), "IdMateria", "NombreMateria");
            return View();
        }

        

        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario), "IdDocente", "Usuario.NombreUsuario");
            ViewBag.Materias = new SelectList(_context.Materias, "IdMateria", "NombreMateria");
            ViewBag.Grupos = new SelectList(_context.Grupos, "IdGrupo", "Grupos");
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

        public IActionResult Detalles(int id)
        {
            var asignacion = _context.AsignacionDocentes
                .Include(a => a.Docente).ThenInclude(d => d.Usuario)
                .Include(a => a.Materia)
                .Include(a => a.Grupo)
                .FirstOrDefault(a => a.Id == id);

            if (asignacion == null)
                return NotFound();

            return View(asignacion);
        }


        //Editar
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var asignacion = _context.AsignacionDocentes.Find(id);
            if (asignacion == null)
                return NotFound();

            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario), "IdDocente", "Usuario.NombreUsuario");
            ViewBag.Materias = new SelectList(_context.Materias, "IdMateria", "NombreMateria");
            ViewBag.Grupos = new SelectList(_context.Grupos, "IdGrupo", "Grupos");
            return View(asignacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(AsignacionDocente asignacion)
        {
            if (ModelState.IsValid)
            {
                _context.AsignacionDocentes.Update(asignacion);
                _context.SaveChanges();
                return RedirectToAction("Lista");
            }

            ViewBag.Docentes = new SelectList(_context.Docentes.Include(d => d.Usuario), "IdDocente", "Usuario.NombreUsuario");
            ViewBag.Materias = new SelectList(_context.Materias, "IdMateria", "NombreMateria");
            ViewBag.Grupos = new SelectList(_context.Grupos, "IdGrupo", "Grupos");
            return View(asignacion);
        }

        //Eliminar
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
