// Controllers/PanelDocenteController.cs
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "2")]    // 2 = Docente
    public class PanelDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PanelDocenteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PanelDocente/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /PanelDocente/CrearAvisos
        [HttpGet]
        public IActionResult CrearAvisos(string modoEnvio, int? idGrupo)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            // Dropdown de grupos
            ViewBag.Grupos = _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} - {dg.Grupo.Grupos}"
                })
                .ToList();

            // Dropdown de materias
            ViewBag.MateriasDocente = _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            ViewBag.ModoEnvio = modoEnvio;
            ViewBag.IdGrupo = idGrupo;

            // Si es individual, cargamos los alumnos del grupo
            if (modoEnvio == "Individual" && idGrupo.HasValue)
            {
                ViewBag.Alumnos = _context.Alumnos
                    .Where(a => a.IdGrupo == idGrupo.Value)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno}"
                    })
                    .ToList();
            }

            return View(new Aviso());
        }

        // POST: /PanelDocente/CrearAvisos
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrearAvisos(Aviso model, string modoEnvio)
        {
            // Recargamos dropdowns igual que en GET
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            ViewBag.Grupos = _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} - {dg.Grupo.Grupos}"
                })
                .ToList();

            ViewBag.MateriasDocente = _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            if (modoEnvio == "Individual" && model.IdGrupo > 0)
            {
                ViewBag.Alumnos = _context.Alumnos
                    .Where(a => a.IdGrupo == model.IdGrupo)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno}"
                    })
                    .ToList();
            }

            if (!ModelState.IsValid)
                return View(model);

            // Asignamos valores fijos
            model.IdDocente = docenteId;
            model.Fecha = DateTime.Now;
            model.TipoAviso = modoEnvio;
            if (modoEnvio == "Grupal")
            {
                model.IdMateria = 0;
                model.MatriculaAlumno = null;
            }

            _context.Avisos.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: /PanelDocente/MisAvisos
        public IActionResult MisAvisos()
        {
            var docenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var avisos = _context.Avisos
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .Include(a => a.Alumno)
                .Where(a => a.IdDocente == docenteId)
                .OrderByDescending(a => a.Fecha)
                .ToList();
            return View(avisos);
        }

        // GET: /PanelDocente/MisAlumnos
        public IActionResult MisAlumnos(int? idGrado, int? idGrupo, int? idMateria)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            // 1) Asignaciones Docente→Grupo
            var asignaciones = _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docenteId)
                .ToList();

            // 2) Dropdown de grados
            ViewBag.Grados = asignaciones
                .GroupBy(dg => dg.IdGrado)
                .Select(g => new SelectListItem
                {
                    Value = g.Key.ToString(),
                    Text = g.First().Grupo!.Grado!.Grados.ToString()
                })
                .ToList();

            // 3) Dropdown de grupos
            ViewBag.Grupos = asignaciones
                .Where(dg => !idGrado.HasValue || dg.IdGrado == idGrado.Value)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = dg.Grupo!.Grupos
                })
                .ToList();

            // 4) Dropdown de materias
            ViewBag.Materias = _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            // 5) Query base de alumnos
            var alumnosQuery = _context.Alumnos
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .Where(a => a.Grupo!.DocentesGrupos
                              .Any(dg => dg.IdDocente == docenteId));

            // 6) Aplicar filtros
            if (idGrado.HasValue) alumnosQuery = alumnosQuery.Where(a => a.IdGrado == idGrado.Value);
            if (idGrupo.HasValue) alumnosQuery = alumnosQuery.Where(a => a.IdGrupo == idGrupo.Value);
            // Nota: Para filtrar por materia necesitarías relacionar Alumnos→Materias

            var alumnos = alumnosQuery.ToList();
            return View(alumnos);
        }
    }
}

