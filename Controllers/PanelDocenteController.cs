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

            vm.Grupos = await _context.AsignacionDocentes
                .Include(ad => ad.Grupo).ThenInclude(g => g.Grado)
                .Where(ad => ad.IdDocente == docente.IdDocente)
                .Select(ad => new SelectListItem
                {
                    Value = ad.IdGrupo.ToString(),
                    Text = $"{ad.Grupo!.Grado!.Grados} – {ad.Grupo.Grupos}"
                })
                .Distinct()
                .ToListAsync();

            vm.Materias = await _context.AsignacionDocentes
                .Include(ad => ad.Materia)
                .Where(ad => ad.IdDocente == docente.IdDocente && ad.IdGrupo == idGrupo)
                .Select(ad => new SelectListItem
                {
                    Value = ad.IdMateria.ToString(),
                    Text = ad.Materia!.NombreMateria
                })
                .Distinct()
                .ToListAsync();

            var grupoIdsAsignados = await _context.AsignacionDocentes
                .Where(ad => ad.IdDocente == docente.IdDocente)
                .Select(ad => ad.IdGrupo)
                .Distinct()
                .ToListAsync();

            if (modoEnvio == "Individual" && idGrupo.HasValue && grupoIdsAsignados.Contains(idGrupo.Value))
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
            else
            {
                vm.Alumnos = new List<SelectListItem>();
            }



            return View(vm);


        }


        // POST: /PanelDocente/CrearAvisos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAvisos(CrearAvisoViewModel vm)
        {
            if (!ModelState.IsValid ||
               (vm.ModoEnvio == "Individual" && !vm.MateriaId.HasValue))
            {
                if (vm.ModoEnvio == "Individual" && !vm.MateriaId.HasValue)
                    ModelState.AddModelError(nameof(vm.MateriaId), "Debes seleccionar una materia.");
                await CargarSelects(vm);
                return View(vm);
            }

            var alumno = await _context.Alumnos
                .Include(a => a.Usuario)
                .Include(a => a.AlumnosTutores)
                    .ThenInclude(at => at.Tutor)
                        .ThenInclude(t => t.Usuario)
                .FirstOrDefaultAsync(a => a.MatriculaAlumno == vm.MatriculaAlumno!.Value);

            if (alumno == null)
            {
                ModelState.AddModelError("", "Alumno no encontrado.");
                await CargarSelects(vm);
                return View(vm);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes.SingleAsync(d => d.IdUsuario == userId);

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

            _context.Avisos.AddRange(avisos);
            await _context.SaveChangesAsync();

            var urlAviso = Url.Action(
                nameof(MisAvisos),
                "PanelDocente",
                values: null,
                protocol: Request.Scheme
            );

            var metodoEnvio = Request.Form["MetodoEnvio"].ToString();

            foreach (var at in alumno.AlumnosTutores)
            {
                var tel = at.Tutor.Telefono?.Trim();
                if (string.IsNullOrEmpty(tel)) continue;

                var numero = tel.StartsWith("1") ? tel : $"1{tel}";
                var destino = metodoEnvio == "SMS"
                    ? $"+52{numero}"
                    : $"whatsapp:+52{numero}";

                try
                {
                    if (metodoEnvio == "WhatsApp")
                    {
                        var vars = new Dictionary<string, string>
                {
                    { "1", at.Tutor.Usuario?.NombreUsuario ?? "Tutor" },
                    { "2", urlAviso ?? "https://birdsing.com" }
                };
                        await _twilio.SendWhatsappAsync(destino, vars);
                    }
                    else if (metodoEnvio == "SMS")
                    {
                        string smsTexto = $"{at.Tutor.Usuario?.NombreUsuario}, nuevo aviso: {vm.Titulo}. Ver: {urlAviso}";
                        if (smsTexto.Length > 160) smsTexto = smsTexto.Substring(0, 160);
                        await _twilio.SendSmsAsync(destino, smsTexto);
                    }
                }
                catch (ApiException ex)
                {
                    _logger.LogError("Twilio API error al enviar a {Destino}: {Code} — {Msg}", destino, ex.Code, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error inesperado al enviar a {Destino}: {Msg}", destino, ex.Message);
                }
            }

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

            vm.Grupos = await _context.AsignacionDocentes
                .Include(ad => ad.Grupo).ThenInclude(g => g.Grado)
                .Where(ad => ad.IdDocente == docente.IdDocente)
                .Select(ad => new SelectListItem
                {
                    Value = ad.IdGrupo.ToString(),
                    Text = $"{ad.Grupo!.Grado!.Grados} – {ad.Grupo.Grupos}"
                })
                .Distinct()
                .ToListAsync();

            vm.Materias = await _context.AsignacionDocentes
                .Include(ad => ad.Materia)
                .Where(ad => ad.IdDocente == docente.IdDocente)
                .Select(ad => new SelectListItem
                {
                    Value = ad.IdMateria.ToString(),
                    Text = ad.Materia!.NombreMateria
                })
                .Distinct()
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
    string? gradoGrupo, int? alumnoId,
    DateTime? fechaDesde, DateTime? fechaHasta, string? nombreAlumno)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Usuario!.IdUsuario == userId);

            if (docente == null)
                return Forbid();

            var idDocente = docente.IdDocente;

            var q = _context.Avisos
                .Include(a => a.Alumno).ThenInclude(a => a.Usuario)
                .Include(a => a.Materia)
                .Include(a => a.Grupo)
                .Where(a => a.IdDocente == idDocente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(gradoGrupo))
            {
                var partes = gradoGrupo.Split('-');
                int idGrado = int.Parse(partes[0]);
                int idGrupo = int.Parse(partes[1]);
                q = q.Where(a => a.Alumno!.IdGrado == idGrado && a.Alumno.IdGrupo == idGrupo);
            }

            if (alumnoId.HasValue)
                q = q.Where(a => a.MatriculaAlumno == alumnoId.Value);

            if (!string.IsNullOrEmpty(nombreAlumno))
            {
                q = q.Where(a =>
                    (a.Alumno!.Usuario != null
                        ? (a.Alumno.Usuario.NombreUsuario + " " + a.Alumno.Usuario.ApellidoPaterno)
                        : (a.Alumno.NombreAlumno + " " + a.Alumno.ApellidoPaterno)
                    ).Contains(nombreAlumno));
            }

            if (fechaDesde.HasValue)
                q = q.Where(a => a.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                q = q.Where(a => a.Fecha <= fechaHasta.Value);

            var avisos = await q.OrderByDescending(a => a.Fecha).ToListAsync();

            var gradosGrupos = await _context.DocentesGrupos
                .Where(dg => dg.IdDocente == idDocente)
                .Select(dg => new SelectListItem
                {
                    Value = dg.IdGrado + "-" + dg.IdGrupo,
                    Text = "Grado " + dg.Grado!.Grados + " - Grupo " + dg.Grupo!.Grupos
                })
                .ToListAsync();

            gradosGrupos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            List<Alumno> alumnosConUsuario;

            if (!string.IsNullOrEmpty(gradoGrupo))
            {
                var partes = gradoGrupo.Split('-');
                int idGrado = int.Parse(partes[0]);
                int idGrupo = int.Parse(partes[1]);

                alumnosConUsuario = await _context.Alumnos
                    .Where(a => a.IdGrado == idGrado && a.IdGrupo == idGrupo)
                    .Include(a => a.Usuario)
                    .ToListAsync();
            }
            else
            {
                alumnosConUsuario = await _context.DocentesGrupos
                    .Where(dg => dg.IdDocente == idDocente)
                    .SelectMany(dg => _context.Alumnos
                        .Where(a => a.IdGrupo == dg.IdGrupo && a.IdGrado == dg.IdGrado)
                        .Include(a => a.Usuario))
                    .Distinct()
                    .ToListAsync();
            }

            var alumnos = alumnosConUsuario
                .Select(a => new SelectListItem
                {
                    Value = a.MatriculaAlumno.ToString(),
                    Text = a.Usuario != null
                        ? $"{a.Usuario.NombreUsuario} {a.Usuario.ApellidoPaterno}"
                        : $"{a.NombreAlumno} {a.ApellidoPaterno}"
                })
                .ToList();

            alumnos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            var vm = new MisAvisosViewModel
            {
                Avisos = avisos,
                Alumnos = alumnos,
                GradosGrupos = gradosGrupos,
                GradoGrupoSeleccionado = gradoGrupo,
                AlumnoId = alumnoId,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                NombreAlumno = nombreAlumno
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
            var grupoIds = await _context.AsignacionDocentes
                .Where(ad => ad.IdDocente == docenteId)
                .Select(ad => ad.IdGrupo)
                .Distinct()
                .ToListAsync();

            var alumnosConUsuario = await _context.Alumnos
            .Include(a => a.Usuario)
            .Include(a => a.Grupo).ThenInclude(g => g.Grado)
            .Where(a => a.IdGrupo.HasValue && grupoIds.Contains(a.IdGrupo.Value))
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
