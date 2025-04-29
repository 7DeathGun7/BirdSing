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
    [Authorize(Roles = "2")] // 2 = Docente
    public class PanelDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PanelDocenteController(ApplicationDbContext context)
            => _context = context;

        // GET: /PanelDocente/Index
        public IActionResult Index()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes
                .Include(d => d.Usuario) // Incluye la información del usuario relacionado
                .FirstOrDefault(d => d.IdUsuario == idUsuario);

            if (docente != null && docente.Usuario != null)
            {
                ViewBag.NombreDocente = docente.Usuario.NombreUsuario + " " + docente.Usuario.ApellidoPaterno; // Combina nombre y apellido
            }
            else
            {
                ViewBag.NombreDocente = "Docente Desconocido"; // En caso de no encontrar el nombre
            }

            return View();
        }

        [HttpGet]
        public IActionResult CrearAvisos(string modoEnvio = "Grupal", int? idGrupo = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.Single(d => d.IdUsuario == userId);

            // prepara el VM
            var vm = new CrearAvisoViewModel
            {
                ModoEnvio = modoEnvio,
                IdGrupo = idGrupo
            };

            // 1) Grupos
            vm.Grupos = _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docente.IdDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} – {dg.Grupo.Grupos}"
                })
                .ToList();

            // 2) Materias
            vm.Materias = _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docente.IdDocente)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            // 3) Si piden envío Individual y ya hay grupo, cargamos Alumnos
            if (modoEnvio == "Individual" && idGrupo.HasValue)
            {
                vm.Alumnos = _context.Alumnos
                    .Where(a => a.IdGrupo == idGrupo.Value)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno}"
                    })
                    .ToList();
            }

            return View(vm);
        }

        // POST: /PanelDocente/CrearAvisos
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrearAvisos(CrearAvisoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // recarga Grupos/Materias/Alumnos idéntico al GET
                return View(vm);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.Single(d => d.IdUsuario == userId);

            var avis = new Aviso
            {
                IdDocente = docente.IdDocente,
                IdGrupo = vm.IdGrupo!.Value,
                MatriculaAlumno = vm.ModoEnvio == "Individual" ? vm.MatriculaAlumno : null,
                TipoAviso = vm.ModoEnvio,
                Titulo = vm.Titulo,
                Mensaje = vm.Mensaje,
                Fecha = DateTime.Now,
                Leido = false,
                // IdMateria lo pones igual que antes...
            };
            _context.Avisos.Add(avis);
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
            // —————————————————————————————————————————————————————————
            // 1) Sacamos el IdUsuario del claim y buscamos al Docente
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes
                           .FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null)
                return Forbid();
            var docenteId = docente.IdDocente;
            // —————————————————————————————————————————————————————————

            // Inicializar ViewModel con los filtros que vienen por query string
            var vm = new MisAlumnosViewModel
            {
                GradoId = gradoId,
                GrupoId = grupoId,
                MateriaId = materiaId
            };

            // 1) Grados que imparte el docente
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

            // 2) Grupos (filtrados opcionalmente por grado)
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

            // 3) Materias (filtradas por grupo, si hay)
            var matQ = _context.MateriasDocentes
                .Where(md => md.IdDocente == docenteId);
            if (grupoId.HasValue)
            {
                var matIds = _context.GrupoMaterias
                    .Where(gm => gm.IdGrupo == grupoId.Value)
                    .Select(gm => gm.IdMateria);
                matQ = matQ.Where(md => matIds.Contains(md.IdMateria));
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

            // 4) Consulta de alumnos según filtros
            var alumnosQ = _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId
                          && (!gradoId.HasValue || dg.IdGrado == gradoId.Value)
                          && (!grupoId.HasValue || dg.IdGrupo == grupoId.Value))
                .SelectMany(dg => dg.Grupo!.Alumnos!)
                .Include(a => a.Usuario)
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .AsQueryable();

            if (materiaId.HasValue)
            {
                var alumnosEnMat = _context.GrupoMaterias
                    .Where(gm => gm.IdMateria == materiaId.Value
                              && (!grupoId.HasValue || gm.IdGrupo == grupoId.Value))
                    .SelectMany(gm => gm.Grupo!.Alumnos!)
                    .Select(a => a.MatriculaAlumno)
                    .Distinct()
                    .ToHashSet();

                alumnosQ = alumnosQ
                    .Where(a => alumnosEnMat.Contains(a.MatriculaAlumno));
            }

            vm.Alumnos = alumnosQ.ToList();
            return View(vm);
        }
        [HttpGet]
        // PanelDocenteController.cs
        [HttpGet]
        public IActionResult GetAlumnos(int idGrupo)
        {
            var lista = _context.Alumnos
                .Where(a => a.IdGrupo == idGrupo)
                .Select(a => new {
                    Matricula = a.MatriculaAlumno,
                    Nombre = a.NombreAlumno + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno
                })
                .ToList();
            return Json(lista);
        }


    }
}

