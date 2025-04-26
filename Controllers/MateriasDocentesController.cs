using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using BirdSing.Models.ModelosViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]
    public class MateriasDocentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MateriasDocentesController(ApplicationDbContext ctx) => _context = ctx;

        public IActionResult ListaMateriasDocentes()
        {
            var lista = _context.MateriasDocentes
                .Include(md => md.Docente).ThenInclude(d => d.Usuario)
                .Include(md => md.Materia)
                .ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult RegistroMateriaDocente()
        {
            CargarDocentesYMaterias();
            return View(new CreateMateriaDocenteViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegistroMateriaDocente(CreateMateriaDocenteViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                CargarDocentesYMaterias();
                return View(vm);
            }
            _context.MateriasDocentes.Add(new MateriaDocente
            {
                IdDocente = vm.IdDocente,
                IdMateria = vm.IdMateria
            });
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMateriasDocentes));
        }

        [HttpGet]
        public IActionResult ActualizarMateriaDocente(int idDocente, int idMateria)
        {
            var entidad = _context.MateriasDocentes
                .Include(md => md.Docente).ThenInclude(d => d.Usuario)
                .Include(md => md.Materia)
                .FirstOrDefault(md => md.IdDocente == idDocente && md.IdMateria == idMateria);
            if (entidad == null) return NotFound();

            CargarDocentesYMaterias();
            return View(new UpdateMateriaDocenteViewModel
            {
                OldIdDocente = entidad.IdDocente,
                OldIdMateria = entidad.IdMateria,
                IdDocente = entidad.IdDocente,
                IdMateria = entidad.IdMateria
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ActualizarMateriaDocente(UpdateMateriaDocenteViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                CargarDocentesYMaterias();
                return View(vm);
            }

            // elimina antigua
            var old = _context.MateriasDocentes.Find(vm.OldIdDocente, vm.OldIdMateria);
            if (old != null) _context.MateriasDocentes.Remove(old);

            // crea nueva
            _context.MateriasDocentes.Add(new MateriaDocente
            {
                IdDocente = vm.IdDocente,
                IdMateria = vm.IdMateria
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(ListaMateriasDocentes));
        }

        public IActionResult EliminarMateriaDocente(int idDocente, int idMateria)
        {
            var e = _context.MateriasDocentes.Find(idDocente, idMateria);
            if (e != null)
            {
                _context.MateriasDocentes.Remove(e);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ListaMateriasDocentes));
        }

        private void CargarDocentesYMaterias()
        {
            ViewBag.Docentes = _context.Docentes
                .Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario!.NombreUsuario
                }).ToList();

            ViewBag.Materias = _context.Materias
                .Select(m => new SelectListItem
                {
                    Value = m.IdMateria.ToString(),
                    Text = m.NombreMateria
                }).ToList();
        }
    }
}
