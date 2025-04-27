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
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Alumnos/ListaAlumnos
        public IActionResult ListaAlumnos()
        {
            var alumnos = _context.Alumnos
                .Include(a => a.Grado)
                .Include(a => a.Grupo)
                .ToList();
            return View(alumnos);
        }

        // GET: /Alumnos/RegistroAlumno
        [HttpGet]
        public IActionResult RegistroAlumno()
        {
            CargarGradosYGrupos();
            return View(new Alumno());
        }

        // POST: /Alumnos/RegistroAlumno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistroAlumno(Alumno model)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosYGrupos();
                return View(model);
            }

            _context.Alumnos.Add(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaAlumnos));
        }

        // GET: /Alumnos/ActualizarAlumno/5
        [HttpGet]
        public IActionResult ActualizarAlumno(int id)
        {
            var alumno = _context.Alumnos
                .Include(a => a.Grado)
                .Include(a => a.Grupo)
                .FirstOrDefault(a => a.MatriculaAlumno == id);

            if (alumno == null)
                return NotFound();

            CargarGradosYGrupos();
            return View(alumno);
        }

        // POST: /Alumnos/ActualizarAlumno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarAlumno(Alumno model)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosYGrupos();
                return View(model);
            }

            // Cargar la entidad existente
            var existente = _context.Alumnos.Find(model.MatriculaAlumno);
            if (existente == null)
                return NotFound();

            // Mapear solo los campos editables
            existente.NombreAlumno = model.NombreAlumno;
            existente.ApellidoPaterno = model.ApellidoPaterno;
            existente.ApellidoMaterno = model.ApellidoMaterno;
            existente.FechaNacimiento = model.FechaNacimiento;
            existente.Curp = model.Curp;
            existente.IdGrado = model.IdGrado;
            existente.IdGrupo = model.IdGrupo;

            _context.SaveChanges();
            return RedirectToAction(nameof(ListaAlumnos));
        }

        // GET: /Alumnos/EliminarAlumno/5
        public IActionResult EliminarAlumno(int id)
        {
            var alumno = _context.Alumnos.Find(id);
            if (alumno != null)
            {
                _context.Alumnos.Remove(alumno);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaAlumnos));
        }

        // Helper para poblar dropdowns de Grados y Grupos
        private void CargarGradosYGrupos()
        {
            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .ToList();

            ViewBag.Grupos = _context.Grupos
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = g.Grupos
                })
                .ToList();
        }
    }
}

