using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo administradores
    public class AlumnosTutoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AlumnosTutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AlumnosTutores/ListaAlumnosTutores
        public IActionResult ListaAlumnosTutores()
        {
            var alumnosTutores = _context.AlumnosTutores
                .Include(at => at.Alumno)
                .Include(at => at.Tutor).ThenInclude(t => t.Usuario)
                .ToList();
            return View(alumnosTutores);
        }

        // GET: /AlumnosTutores/RegistroAlumnoTutor
        [HttpGet]
        public IActionResult RegistroAlumnoTutor()
        {
            // Carga alumnos
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();

            // Carga tutores INCLUYENDO su Usuario para que NombreUsuario no sea null
            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                .ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario!.NombreUsuario
            }).ToList();

            return View(new AlumnoTutor());
        }

        // POST: /AlumnosTutores/RegistroAlumnoTutor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistroAlumnoTutor(AlumnoTutor model)
        {
            if (ModelState.IsValid)
            {
                var existe = _context.AlumnosTutores.Any(at =>
                    at.MatriculaAlumno == model.MatriculaAlumno &&
                    at.IdTutor == model.IdTutor);

                if (!existe)
                {
                    _context.AlumnosTutores.Add(model);
                    _context.SaveChanges();
                    TempData["mensaje"] = "Tutor asignado correctamente.";
                    return RedirectToAction(nameof(ListaAlumnosTutores)); // ✅ Redirige si se guardó correctamente
                }
                else
                {
                    ViewBag.Mensaje = "Este tutor ya está asignado a este alumno.";
                }
            }

            // Si hubo error o ya existía, recarga la vista actual
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();

            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                .ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario!.NombreUsuario
            }).ToList();

            return View(model); // 🔁 Se mantiene en el formulario
        }


        // GET: /AlumnosTutores/ActualizarAlumnoTutor?matriculaAlumno=1&idTutor=2
        [HttpGet]
        public IActionResult ActualizarAlumnoTutor(int matriculaAlumno, int idTutor)
        {
            var at = _context.AlumnosTutores
                .Include(x => x.Alumno)
                .Include(x => x.Tutor).ThenInclude(t => t.Usuario)
                .FirstOrDefault(x => x.MatriculaAlumno == matriculaAlumno && x.IdTutor == idTutor);
            if (at == null) return NotFound();

            // Recargamos dropdowns
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();

            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                .ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario!.NombreUsuario
            }).ToList();

            return View(at);
        }

        // POST: /AlumnosTutores/ActualizarAlumnoTutor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarAlumnoTutor(AlumnoTutor model)
        {
            if (ModelState.IsValid)
            {
                _context.AlumnosTutores.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(ListaAlumnosTutores));
            }

            // Recarga dropdowns en caso de error
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();

            var tutores = _context.Tutores
                .Include(t => t.Usuario)
                .ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario!.NombreUsuario
            }).ToList();

            return View(model);
        }

        // GET: /AlumnosTutores/EliminarAlumnoTutor?matriculaAlumno=1&idTutor=2
        public IActionResult EliminarAlumnoTutor(int matriculaAlumno, int idTutor)
        {
            var at = _context.AlumnosTutores
                .FirstOrDefault(x => x.MatriculaAlumno == matriculaAlumno && x.IdTutor == idTutor);
            if (at != null)
            {
                _context.AlumnosTutores.Remove(at);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaAlumnosTutores));
        }
    }
}
