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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "3")] // 3 = Tutor
    public class PanelTutorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanelTutorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PanelTutor/Index
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Incluimos navegación a Usuario para mostrar nombre
            var tutor = await _context.Tutores
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Usuario!.IdUsuario == userId);

            ViewBag.NombreTutor = tutor?.Usuario != null
                ? $"{tutor.Usuario.NombreUsuario} {tutor.Usuario.ApellidoPaterno}"
                : "Tutor Desconocido";

            return View();
        }

        // GET: /PanelTutor/MisAvisos
        public async Task<IActionResult> MisAvisos(
            int? alumnoId,
            bool? leido,
            DateTime? fechaDesde,
            DateTime? fechaHasta
        )
        {
            // 1) Obtén al tutor y sus alumnos con sus datos de usuario
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var tutor = await _context.Tutores
                .Include(t => t.AlumnosTutores)
                    .ThenInclude(at => at.Alumno)
                        .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(t => t.Usuario!.IdUsuario == userId);

            if (tutor == null)
                return Forbid();

            // 2) Dropdown Alumnos: proyectamos primero a SelectListItem
            var listaAlumnos = tutor.AlumnosTutores
                .Select(at => at.Alumno!)
                .Select(a => new SelectListItem
                {
                    Value = a.MatriculaAlumno.ToString(),
                    Text = a.Usuario != null
                        ? $"{a.Usuario.NombreUsuario} {a.Usuario.ApellidoPaterno}"
                        : $"{a.NombreAlumno} {a.ApellidoPaterno}"
                })
                .OrderBy(x => x.Text)
                .ToList();
            listaAlumnos.Insert(0, new SelectListItem { Value = "", Text = "-- Todos --" });

            // 3) Construye Queryable de avisos, filtrando sólo los de sus alumnos
            var q = _context.Avisos
                .Include(a => a.Alumno).ThenInclude(al => al.Usuario)
                .Where(a => tutor.AlumnosTutores
                                 .Select(at => at.Alumno.MatriculaAlumno)
                                 .Contains(a.MatriculaAlumno!.Value))
                .AsQueryable();

            if (alumnoId.HasValue)
                q = q.Where(a => a.MatriculaAlumno == alumnoId.Value);

            if (leido.HasValue)
                q = q.Where(a => a.Leido == leido.Value);

            if (fechaDesde.HasValue)
                q = q.Where(a => a.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                q = q.Where(a => a.Fecha <= fechaHasta.Value);

            var avisos = await q
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            // 4) Prepara lista de estados para el dropdown de "Leído"
            var estados = new List<SelectListItem> {
                new SelectListItem{ Value = "",      Text = "-- Todos --" },
                new SelectListItem{ Value = "true",  Text = "Leído"       },
                new SelectListItem{ Value = "false", Text = "No leído"    }
            };

            // 5) Monta el ViewModel y pásalo a la vista
            var vm = new MisAvisosTutorViewModel
            {
                Alumnos = listaAlumnos,
                Estados = estados,
                Avisos = avisos,
                AlumnoId = alumnoId,
                Leido = leido,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };

            return View(vm);
        }
        public async Task<IActionResult> Detalles(int id)
        {
            var aviso = await _context.Avisos
                .Include(a => a.Materia)
                .Include(a => a.Alumno).ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aviso == null) return NotFound();

            if (!aviso.Leido)
            {
                aviso.Leido = true;
                await _context.SaveChangesAsync();
            }

            return View(aviso);
        }

        // GET: /PanelTutor/InformacionTutor
        public async Task<IActionResult> InformacionTutor()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var tutor = await _context.Tutores
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.IdUsuario == userId);

            if (tutor == null) return NotFound();

            return View(tutor); // Buscará "InformacionTutor.cshtml"
        }
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null) return NotFound();

            // Verifica contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(vm.ContrasenaActual, usuario.Password))
            {
                ModelState.AddModelError("ContrasenaActual", "La contraseña actual no es correcta.");
                return View(vm);
            }

            // Cambia y guarda
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(vm.NuevaContrasena);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("InformacionTutor");
        }


    }
}
