using BirdSing.Data;
using BirdSing.Models;
using BirdSing.Models.ViewModels;
using BirdSing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "2")] // 2 = Docente
    public class PanelDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITwilioService _twilio;

        public PanelDocenteController(
            ApplicationDbContext context,
            ITwilioService twilio
        )
        {
            _context = context;
            _twilio = twilio;
        }

        // GET: /PanelDocente/Index
        public IActionResult Index()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefault(d => d.IdUsuario == idUsuario);

            ViewBag.NombreDocente = docente?.Usuario != null
                ? $"{docente.Usuario.NombreUsuario} {docente.Usuario.ApellidoPaterno}"
                : "Docente Desconocido";

            return View();
        }

        // GET: /PanelDocente/CrearAvisos
        [HttpGet]
        public IActionResult CrearAvisos(string modoEnvio = "Grupal", int? idGrupo = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.Single(d => d.IdUsuario == userId);

            var vm = new CrearAvisoViewModel
            {
                ModoEnvio = modoEnvio,
                IdGrupo = idGrupo
            };

            // 1) Grupos del docente
            vm.Grupos = _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docente.IdDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} – {dg.Grupo.Grupos}"
                })
                .ToList();

            // 2) Materias que imparte
            vm.Materias = _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docente.IdDocente)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToList();

            // 3) Alumnos (solo si es envío Individual y ya vino IdGrupo)
            if (modoEnvio == "Individual" && idGrupo.HasValue)
            {
                vm.Alumnos = _context.Alumnos
                    .Where(a => a.IdGrupo == idGrupo.Value)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                    })
                    .ToList();
            }

            return View(vm);
        }

        // POST: /PanelDocente/CrearAvisos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAvisos(CrearAvisoViewModel vm)
        {
            // validación clásica de Razor
            if (!ModelState.IsValid)
            {
                await CargarSelects(vm);
                return View(vm);
            }

            // + esta validación extra para el caso individual
            if (vm.ModoEnvio == "Individual" && !vm.MateriaId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.MateriaId), "Debes seleccionar una materia.");
                await CargarSelects(vm);
                return View(vm);
            }


            // 2) Cargar el alumno junto con sus tutores
            var alumno = await _context.Alumnos
                .Include(a => a.Usuario)
                .Include(a => a.AlumnosTutores)
                    .ThenInclude(at => at.Tutor)
                .FirstOrDefaultAsync(a => a.MatriculaAlumno == vm.MatriculaAlumno!.Value);

            if (alumno == null)
            {
                ModelState.AddModelError("", "Alumno no encontrado.");
                await CargarSelects(vm);
                return View(vm);
            }

            // 3) Obtener al docente actual
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes
                .SingleAsync(d => d.IdUsuario == userId);

            // 4) Construir lista de Avisos: uno por cada tutor del alumno
            var avisos = alumno.AlumnosTutores
                .Select(at => new Aviso
                {
                    IdDocente = docente.IdDocente,
                    IdGrupo = vm.IdGrupo!.Value,
                    // GetValueOrDefault() evita la excepción Nullabe.Value
                    IdMateria = vm.ModoEnvio == "Individual"
                                     ? vm.MateriaId.GetValueOrDefault()
                                     : 0,
                    MatriculaAlumno = vm.MatriculaAlumno,
                    IdTutor = at.IdTutor,
                    TipoAviso = vm.ModoEnvio,
                    Titulo = vm.Titulo,
                    Mensaje = vm.Mensaje,
                    Fecha = DateTime.Now,
                    Leido = false
                })
                .ToList();

            // 5) Guardar en base de datos
            _context.Avisos.AddRange(avisos);
            await _context.SaveChangesAsync();

            // 6) Preparar la URL y el texto para WhatsApp
            var nombreAlumno = alumno.Usuario != null
                ? $"{alumno.Usuario.NombreUsuario} {alumno.Usuario.ApellidoPaterno}"
                : $"{alumno.NombreAlumno} {alumno.ApellidoPaterno}";

            var urlAviso = Url.Action(
                nameof(MisAvisos),
                "PanelDocente",
                null,
                Request.Scheme
            );

            var texto = $"Se asignó un nuevo aviso a {nombreAlumno}.\n" +
                        $"Ingresa al sistema para verlo:\n{urlAviso}";

            // 7) Enviar WhatsApp a cada tutor
            foreach (var at in alumno.AlumnosTutores)
            {
                var tel = at.Tutor.Telefono?.Trim();
                if (!string.IsNullOrEmpty(tel))
                {
                    var destino = $"whatsapp:+52{tel}";
                    await _twilio.SendWhatsappAsync(destino, texto);
                }
            }

            // 8) Mensaje de éxito y redirección
            TempData["Success"] = "Aviso enviado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Rellena los dropdowns de Grupos, Materias y Alumnos
        /// cuando el modelo no es válido y hay que volver a mostrar la vista.
        /// </summary>
        private async Task CargarSelects(CrearAvisoViewModel vm)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes.SingleAsync(d => d.IdUsuario == userId);

            // Grupos
            vm.Grupos = await _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docente.IdDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} – {dg.Grupo.Grupos}"
                })
                .ToListAsync();

            // Materias
            vm.Materias = await _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docente.IdDocente)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToListAsync();

            // Alumnos (solo en individual)
            if (vm.ModoEnvio == "Individual" && vm.IdGrupo.HasValue)
            {
                vm.Alumnos = await _context.Alumnos
                    .Where(a => a.IdGrupo == vm.IdGrupo)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                    })
                    .ToListAsync();
            }
        }

        /// GET: /PanelDocente/MisAvisos
        [HttpGet]
        public async Task<IActionResult> MisAvisos(
            int? materiaId,
            int? alumnoId,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes
                                        .FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

            // 1) Cargar listas para dropdowns
            var vm = new MisAvisosViewModel
            {
                MateriaId = materiaId,
                AlumnoId = alumnoId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };

            // Materias del docente
            vm.Materias = await _context.MateriasDocentes
                .Where(md => md.IdDocente == docenteId)
                .Select(md => md.Materia!)
                .Distinct()
                .Select(m => new SelectListItem
                {
                    Value = m.IdMateria.ToString(),
                    Text = m.NombreMateria
                })
                .OrderBy(x => x.Text)
                .ToListAsync();
            vm.Materias.Insert(0, new SelectListItem { Value = "", Text = "-- Todas --" });

            // Alumnos del docente (todos los grupos que imparte)
            var grupoIds = await _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => dg.IdGrupo)
                .ToListAsync();

            vm.Alumnos = await _context.Alumnos
                .Include(a => a.Usuario)
                .Where(a => grupoIds.Contains(a.IdGrupo))
                .Select(a => new SelectListItem
                {
                    Value = a.MatriculaAlumno.ToString(),
                    Text = $"{a.Usuario!.NombreUsuario} {a.Usuario.ApellidoPaterno}"
                })
                .OrderBy(x => x.Text)
                .ToListAsync();
            vm.Alumnos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 2) Construir query de avisos
            var q = _context.Avisos
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .Include(a => a.Materia)
                .Include(a => a.Alumno).ThenInclude(al => al.Usuario)
                .Where(a => a.IdDocente == docenteId)
                .AsQueryable();

            if (materiaId.HasValue)
                q = q.Where(a => a.IdMateria == materiaId.Value);

            if (alumnoId.HasValue)
                q = q.Where(a => a.MatriculaAlumno == alumnoId.Value);

            if (fechaDesde.HasValue)
                q = q.Where(a => a.Fecha >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                q = q.Where(a => a.Fecha <= fechaHasta.Value.Date.AddDays(1).AddTicks(-1));

            vm.Avisos = await q
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            return View(vm);
        }

        // GET: /PanelDocente/MisAlumnos
        public IActionResult MisAlumnos(int? gradoId, int? grupoId, int? materiaId)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes.FirstOrDefault(d => d.IdUsuario == idUsuario);
            if (docente == null) return Forbid();
            var docenteId = docente.IdDocente;

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

            // 2) Grupos
            var gruposQ = _context.DocentesGrupos.Where(dg => dg.IdDocente == docenteId);
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

            // 3) Materias
            var matQ = _context.MateriasDocentes.Where(md => md.IdDocente == docenteId);
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

            // 4) Alumnos
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

                alumnosQ = alumnosQ.Where(a => alumnosEnMat.Contains(a.MatriculaAlumno));
            }

            vm.Alumnos = alumnosQ.ToList();
            return View(vm);
        }

        // GET para cargar alumnos vía AJAX
        [HttpGet]
        public IActionResult GetAlumnos(int idGrupo)
        {
            var lista = _context.Alumnos
                .Where(a => a.IdGrupo == idGrupo)
                .Select(a => new {
                    Matricula = a.MatriculaAlumno,
                    Nombre = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                })
                .ToList();
            return Json(lista);
        }
    }
}
