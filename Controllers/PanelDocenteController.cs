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
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twilio.Exceptions;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "2")] // 2 = Docente
    public class PanelDocenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITwilioService _twilio;
        private readonly ILogger<PanelDocenteController> _logger;

        public PanelDocenteController(
            ApplicationDbContext context,
            ITwilioService twilio,
            ILogger<PanelDocenteController> logger
        )
        {
            _context = context;
            _twilio = twilio;
            _logger = logger;
        }

        // GET: /PanelDocente/Index
        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefault(d => d.IdUsuario == userId);

            ViewBag.NombreDocente = docente?.Usuario != null
                ? $"{docente.Usuario.NombreUsuario} {docente.Usuario.ApellidoPaterno}"
                : "Docente Desconocido";

            return View();
        }

        // GET: /PanelDocente/CrearAvisos
        [HttpGet]
        public async Task<IActionResult> CrearAvisos(string modoEnvio = "Grupal", int? idGrupo = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes.SingleAsync(d => d.IdUsuario == userId);

            var vm = new CrearAvisoViewModel
            {
                ModoEnvio = modoEnvio,
                IdGrupo = idGrupo
            };

            // 1) Grupos
            vm.Grupos = await _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docente.IdDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} – {dg.Grupo.Grupos}"
                })
                .ToListAsync();

            // 2) Materias
            vm.Materias = await _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docente.IdDocente)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToListAsync();

            // 3) Alumnos (si modo=Individual y ya pasaron grupo)
            if (modoEnvio == "Individual" && idGrupo.HasValue)
            {
                vm.Alumnos = await _context.Alumnos
                    .Where(a => a.IdGrupo == idGrupo.Value)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                    })
                    .ToListAsync();
            }

            return View(vm);
        }


        // POST: /PanelDocente/CrearAvisos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAvisos(CrearAvisoViewModel vm)
        {
            // 1) Validaciones
            if (!ModelState.IsValid ||
               (vm.ModoEnvio == "Individual" && !vm.MateriaId.HasValue))
            {
                if (vm.ModoEnvio == "Individual" && !vm.MateriaId.HasValue)
                    ModelState.AddModelError(nameof(vm.MateriaId), "Debes seleccionar una materia.");
                await CargarSelects(vm);
                return View(vm);
            }

            // 2) Carga alumno + tutores
            var alumno = await _context.Alumnos
                .Include(a => a.Usuario)
                .Include(a => a.AlumnosTutores).ThenInclude(at => at.Tutor)
                .FirstOrDefaultAsync(a => a.MatriculaAlumno == vm.MatriculaAlumno!.Value);

            if (alumno == null)
            {
                ModelState.AddModelError("", "Alumno no encontrado.");
                await CargarSelects(vm);
                return View(vm);
            }

            // 3) Docente actual
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes.SingleAsync(d => d.IdUsuario == userId);

            // 4) Construye lista de Avisos
            var avisos = alumno.AlumnosTutores
                .Select(at => new Aviso
                {
                    IdDocente = docente.IdDocente,
                    IdGrupo = vm.IdGrupo!.Value,
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

            // 5) Guarda
            _context.Avisos.AddRange(avisos);
            await _context.SaveChangesAsync();

            // 6) Prepara plantilla de WhatsApp
            var nombreAlumno = alumno.Usuario != null
                ? $"{alumno.Usuario.NombreUsuario} {alumno.Usuario.ApellidoPaterno}"
                : $"{alumno.NombreAlumno} {alumno.ApellidoPaterno}";

            var urlAviso = Url.Action(
                nameof(MisAvisos),
                "PanelDocente",
                values: null,
                protocol: Request.Scheme
            );
            // 7) Envío a cada tutor con template y log de errores
            foreach (var at in alumno.AlumnosTutores)
            {
                var tel = at.Tutor.Telefono?.Trim();
                if (string.IsNullOrEmpty(tel)) continue;

                var destino = $"whatsapp:+52{tel}";
                // Lista de variables para {{1}} y {{2}} de tu plantilla
                var vars = new List<string> { nombreAlumno, urlAviso };

                try
                {
                    await _twilio.SendWhatsappAsync(destino, vars);
                }
                catch (ApiException ex)
                {
                    _logger.LogError("Twilio API error al enviar a {Destino}: {Code} — {Msg}",
                                     destino, ex.Code, ex.Message);
                }
                catch (TwilioException ex)
                {
                    _logger.LogError("Twilio error al enviar a {Destino}: {Msg}",
                                     destino, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error inesperado al enviar a {Destino}: {Msg}",
                                     destino, ex.Message);
                }
            }


            // 8) Éxito y redirección
            TempData["Success"] = "Aviso enviado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Rellena dropdowns de Grupos, Materias y Alumnos para volver a mostrar la vista
        /// cuando el modelo no es válido.
        /// </summary>
        private async Task CargarSelects(CrearAvisoViewModel vm)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes.SingleAsync(d => d.IdUsuario == userId);

            vm.Grupos = await _context.DocentesGrupos
                .Include(dg => dg.Grupo).ThenInclude(g => g.Grado)
                .Where(dg => dg.IdDocente == docente.IdDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrupo.ToString(),
                    Text = $"{dg.Grupo!.Grado!.Grados} – {dg.Grupo.Grupos}"
                })
                .ToListAsync();

            vm.Materias = await _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docente.IdDocente)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .ToListAsync();

            if (vm.ModoEnvio == "Individual" && vm.IdGrupo.HasValue)
            {
                vm.Alumnos = await _context.Alumnos
                    .Where(a => a.IdGrupo == vm.IdGrupo.Value)
                    .Select(a => new SelectListItem
                    {
                        Value = a.MatriculaAlumno.ToString(),
                        Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                    })
                    .ToListAsync();
            }
        }

        // GET: /PanelDocente/MisAvisos
        public async Task<IActionResult> MisAvisos(
            int? materiaId,
            int? alumnoId,
            DateTime? fechaDesde,
            DateTime? fechaHasta
        )
        {
            var docenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1) Dropdown Materias
            var materias = await _context.MateriasDocentes
                .Include(md => md.Materia)
                .Where(md => md.IdDocente == docenteId)
                .Select(md => new SelectListItem
                {
                    Value = md.IdMateria.ToString(),
                    Text = md.Materia!.NombreMateria
                })
                .OrderBy(x => x.Text)
                .ToListAsync();
            materias.Insert(0, new SelectListItem { Value = "", Text = "-- Todas --" });

            // 2) Dropdown Alumnos (misma técnica en memoria)
            var grupoIds = await _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => dg.IdGrupo)
                .ToListAsync();

            var alumnosConUsuario = await _context.Alumnos
                .Include(a => a.Usuario)
                .Include(a => a.Grupo)                // <— carga la entidad Grupo
                   .ThenInclude(g => g.Grado)        // <— y de paso carga el Grado
                .Where(a => grupoIds.Contains(a.IdGrupo))
                .ToListAsync();

            var alumnosList = alumnosConUsuario
                .Select(a => new SelectListItem
                {
                    Value = a.MatriculaAlumno.ToString(),
                    Text = a.Usuario != null
                            ? $"{a.Usuario.NombreUsuario} {a.Usuario.ApellidoPaterno}"
                            : $"{a.NombreAlumno} {a.ApellidoPaterno}"
                })
                .OrderBy(x => x.Text)
                .ToList();
            alumnosList.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 3) Consulta de avisos con filtros
            var query = _context.Avisos
                .Include(a => a.Alumno).ThenInclude(a => a.Usuario)
                .Include(a => a.Materia)
                .Where(a => a.IdDocente == docenteId)
                .AsQueryable();

            if (materiaId.HasValue) query = query.Where(a => a.IdMateria == materiaId.Value);
            if (alumnoId.HasValue) query = query.Where(a => a.MatriculaAlumno == alumnoId.Value);
            if (fechaDesde.HasValue) query = query.Where(a => a.Fecha >= fechaDesde.Value);
            if (fechaHasta.HasValue) query = query.Where(a => a.Fecha <= fechaHasta.Value);

            var avisos = await query
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            // 4) Pasa todo al ViewModel
            var vm = new MisAvisosViewModel
            {
                Materias = materias,
                AlumnosList = alumnosList,
                Avisos = avisos,
                MateriaId = materiaId,
                AlumnoId = alumnoId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };

            return View(vm);
        }

        // GET: /PanelDocente/MisAlumnos
        public async Task<IActionResult> MisAlumnos(
    int? gradoId,
    int? grupoId,
    int? materiaId,
    int? alumnoId     // <-- nuevo parámetro para el filtro de alumno
)
        {
            // 1) Obtenemos al docente
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes
                                  .FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null)
                return Forbid();
            var docenteId = docente.IdDocente;

            // 2) Preparamos el viewmodel
            var vm = new MisAlumnosViewModel
            {
                GradoId = gradoId,
                GrupoId = grupoId,
                MateriaId = materiaId,
                AlumnoId = alumnoId
            };

            // 3) Dropdown Grados
            vm.Grados = await _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => dg.Grupo!.Grado!)
                .Distinct()
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .OrderBy(x => x.Text)
                .ToListAsync();
            vm.Grados.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 4) Dropdown Grupos (filtrado por grado opcional)
            var gruposQ = _context.DocentesGrupos.Where(dg => dg.IdDocente == docenteId);
            if (gradoId.HasValue)
                gruposQ = gruposQ.Where(dg => dg.IdGrado == gradoId.Value);

            vm.Grupos = await gruposQ
                .Select(dg => dg.Grupo!)
                .Distinct()
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = g.Grupos
                })
                .OrderBy(x => x.Text)
                .ToListAsync();
            vm.Grupos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 5) Dropdown Materias (filtrado por grupo opcional)
            var matQ = _context.MateriasDocentes.Where(md => md.IdDocente == docenteId);
            if (grupoId.HasValue)
            {
                var matIds = await _context.GrupoMaterias
                    .Where(gm => gm.IdGrupo == grupoId.Value)
                    .Select(gm => gm.IdMateria)
                    .ToListAsync();
                matQ = matQ.Where(md => matIds.Contains(md.IdMateria));
            }

            vm.Materias = await matQ
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

            // 6) Obtengo en memoria todos los alumnos de los grupos del docente
            var grupoIds = await _context.DocentesGrupos
                .Where(dg => dg.IdDocente == docenteId)
                .Select(dg => dg.IdGrupo)
                .ToListAsync();

            var alumnosConUsuario = await _context.Alumnos
                .Include(a => a.Usuario)
                .Include(a => a.Grupo)             // carga la entidad Grupo
                   .ThenInclude(g => g.Grado)     // y su Grado
                .Where(a => grupoIds.Contains(a.IdGrupo))
                .ToListAsync();

            // 7) Dropdown Alumnos
            vm.AlumnosList = alumnosConUsuario
                .Select(a => new SelectListItem
                {
                    Value = a.MatriculaAlumno.ToString(),
                    Text = a.Usuario != null
                            ? $"{a.Usuario.NombreUsuario} {a.Usuario.ApellidoPaterno}"
                            : $"{a.NombreAlumno} {a.ApellidoPaterno}"
                })
                .OrderBy(x => x.Text)
                .ToList();
            vm.AlumnosList.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 8) Ahora filtro la propia lista de alumnos en memoria
            var tablaQ = alumnosConUsuario.AsQueryable();
            if (gradoId.HasValue) tablaQ = tablaQ.Where(a => a.Grupo!.IdGrado == gradoId.Value);
            if (grupoId.HasValue) tablaQ = tablaQ.Where(a => a.IdGrupo == grupoId.Value);
            if (materiaId.HasValue)
            {
                var matrEnMat = await _context.GrupoMaterias
                    .Where(gm => gm.IdMateria == materiaId.Value)
                    .SelectMany(gm => gm.Grupo!.Alumnos!)
                    .Select(a => a.MatriculaAlumno)
                    .ToListAsync();
                tablaQ = tablaQ.Where(a => matrEnMat.Contains(a.MatriculaAlumno));
            }
            if (alumnoId.HasValue)
                tablaQ = tablaQ.Where(a => a.MatriculaAlumno == alumnoId.Value);

            vm.Alumnos = tablaQ.ToList();

            // 9) **MUY IMPORTANTE**: devolver la vista
            return View(vm);
        }

        // GET: /PanelDocente/GetAlumnos?idGrupo=...
        [HttpGet]
        public IActionResult GetAlumnos(int idGrupo)
        {
            var lista = _context.Alumnos
                .Where(a => a.IdGrupo == idGrupo)
                .Select(a => new {
                    matricula = a.MatriculaAlumno,
                    nombre = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
                })
                .ToList();
            return Json(lista);
        }
    }
}
