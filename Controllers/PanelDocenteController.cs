using BirdSing.Data;
using BirdSing.Models;
using BirdSing.Models.ViewModels;
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
            => _context = context;


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

        public IActionResult MisAlumnos(int? gradoId, int? grupoId, int? materiaId)
        {
            var docenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var vm = new MisAlumnosViewModel
            {
                GradoId = gradoId,
                GrupoId = grupoId,
                MateriaId = materiaId
            };

            // 1) Grados
            vm.Grados = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => dg.Grupo!.Grado!)
                .Distinct()
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .OrderBy(x => x.Text)
                .ToList();
            vm.Grados.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 2) Grupos (opcionalmente filtrados por grado)
            var gruposQ = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId);
            if (gradoId.HasValue)
                gruposQ = gruposQ.Where(dg => dg.IdGrado == gradoId.Value);

            vm.Grupos = gruposQ
                .Select(dg => dg.Grupo!)
                .Distinct()
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = g.Grupos
                })
                .OrderBy(x => x.Text)
                .ToList();
            vm.Grupos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 3) Materias (opcionalmente filtradas por grupo)
            var matQ = _context.MateriasDocentes
                .Where(md => md.IdDocente == docenteId);
            if (grupoId.HasValue)
            {
                var grupoMaterias = _context.GrupoMaterias
                    .Where(gm => gm.IdGrupo == grupoId.Value)
                    .Select(gm => gm.IdMateria);
                matQ = matQ.Where(md => grupoMaterias.Contains(md.IdMateria));
            }
            vm.Materias = matQ
                .Select(md => md.Materia!)
                .Distinct()
                .Select(m => new SelectListItem
                {
                    Value = m.IdMateria.ToString(),
                    Text = m.NombreMateria
                })
                .OrderBy(x => x.Text)
                .ToList();
            vm.Materias.Insert(0, new SelectListItem { Value = "", Text = "-- Todas --" });

            // 4) Consulta alumnos según filtros
            var dgBase = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId);

            if (gradoId.HasValue)
                dgBase = dgBase.Where(dg => dg.IdGrado == gradoId.Value);
            if (grupoId.HasValue)
                dgBase = dgBase.Where(dg => dg.IdGrupo == grupoId.Value);

            var alumnos = dgBase
                .SelectMany(dg => dg.Grupo!.Alumnos!)
                .Include(a => a.Usuario)
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .ToList();

            if (materiaId.HasValue)
            {
                // sólo los alumnos inscritos en esa materia
                var alumnosEnMat = _context.GrupoMaterias
                    .Where(gm => gm.IdMateria == materiaId.Value
                              && (!grupoId.HasValue || gm.IdGrupo == grupoId.Value))
                    .SelectMany(gm => gm.Grupo!.Alumnos!)
                    .Select(a => a.MatriculaAlumno)
                    .Distinct()
                    .ToHashSet();

                alumnos = alumnos
                    .Where(a => alumnosEnMat.Contains(a.MatriculaAlumno))
                    .ToList();
            }

            vm.Alumnos = alumnos;
            return View(vm);
        }
    }
}

