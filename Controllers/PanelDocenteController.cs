using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Security.Claims;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "2")]
    public class PanelDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanelDocenteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PanelDocente/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: PanelDocente/CrearAviso
        [HttpGet]
        public IActionResult CrearAvisos()
        {
            // 1) Leer el IdUsuario desde el claim NameIdentifier
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 2) Buscar el Docente asociado a ese usuario
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            // 3) Poblar dropdown de Grado–Grupo (value=IdGrupo, text="1-A", etc)
            ViewBag.Grupos = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grado!.Grados}-{dg.Grupo!.Grupos}"
                })
                .ToList();

            // 4) Poblar dropdown de Materias que imparte
            ViewBag.MateriasDocente = _context.MateriasDocentes
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            return View(new Aviso());
        }

        // POST: PanelDocente/CrearAviso
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrearAvisos(Aviso model, string modoEnvio)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            // recargar dropdowns si hay validación fallida
            ViewBag.Grupos = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grado!.Grados}-{dg.Grupo!.Grupos}"
                })
                .ToList();
            ViewBag.MateriasDocente = _context.MateriasDocentes
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            if (!ModelState.IsValid)
                return View(model);

            // asignar los campos fijos
            model.IdDocente = docenteId;
            model.Fecha = DateTime.Now;
            model.Leido = false;
            model.TipoAviso = modoEnvio;  // "Grupal" o "Individual"

            // si es grupal, no usamos materia ni alumno
            if (modoEnvio == "Grupal")
            {
                model.IdMateria = 0;
                model.MatriculaAlumno = null;
            }

            _context.Avisos.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
