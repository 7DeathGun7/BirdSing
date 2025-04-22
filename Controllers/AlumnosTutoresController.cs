using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class AlumnosTutoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosTutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaAlumnosTutores()
        {
            var alumnosTutores = _context.AlumnosTutores.Include(at => at.Alumno).Include(at => at.Tutor).ToList();
            return View(alumnosTutores);
        }

        public IActionResult RegistroAlumnoTutor()
        {
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();
            var tutores = _context.Tutores.ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario.NombreUsuario
            }).ToList();
            return View(new AlumnoTutor());
        }

        [HttpPost]
        public IActionResult RegistroAlumnoTutor(AlumnoTutor model)
        {
            if (ModelState.IsValid)
            {
                _context.AlumnosTutores.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaAlumnosTutores");
            }
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();
            var tutores = _context.Tutores.ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario.NombreUsuario
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarAlumnoTutor(int matriculaAlumno, int idTutor)
        {
            var alumnoTutor = _context.AlumnosTutores.Include(at => at.Alumno).Include(at => at.Tutor).FirstOrDefault(at => at.MatriculaAlumno == matriculaAlumno && at.IdTutor == idTutor);
            if (alumnoTutor == null)
            {
                return NotFound();
            }
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();
            var tutores = _context.Tutores.ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario.NombreUsuario
            }).ToList();
            return View(alumnoTutor);
        }

        [HttpPost]
        public IActionResult ActualizarAlumnoTutor(AlumnoTutor model)
        {
            if (ModelState.IsValid)
            {
                _context.AlumnosTutores.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaAlumnosTutores");
            }
            var alumnos = _context.Alumnos.ToList();
            ViewBag.Alumnos = alumnos.Select(a => new SelectListItem
            {
                Value = a.MatriculaAlumno.ToString(),
                Text = $"{a.NombreAlumno} {a.ApellidoPaterno} {a.ApellidoMaterno}"
            }).ToList();
            var tutores = _context.Tutores.ToList();
            ViewBag.Tutores = tutores.Select(t => new SelectListItem
            {
                Value = t.IdTutor.ToString(),
                Text = t.Usuario.NombreUsuario
            }).ToList();
            return View(model);
        }

        public IActionResult EliminarAlumnoTutor(int matriculaAlumno, int idTutor)
        {
            var alumnoTutor = _context.AlumnosTutores.FirstOrDefault(at => at.MatriculaAlumno == matriculaAlumno && at.IdTutor == idTutor);
            if (alumnoTutor != null)
            {
                _context.AlumnosTutores.Remove(alumnoTutor);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaAlumnosTutores");
        }
    }
}