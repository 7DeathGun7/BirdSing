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
    [Authorize(Roles = "1")]  // Solo administradores
    public class GruposController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GruposController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Grupos/ListaGrupos
        public IActionResult ListaGrupos()
        {
            var grupos = _context.Grupos
                .Include(g => g.Grado)
                .ToList();
            return View(grupos);
        }

        // GET: Grupos/RegistroGrupo
        [HttpGet]
        public IActionResult RegistroGrupo()
        {
            CargarGradosEnViewBag();
            return View(new GradoGrupoViewModel());
        }

        // POST: Grupos/RegistroGrupo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistroGrupo(GradoGrupoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(vm);
            }

            var grupo = new Grupo
            {
                IdGrado = vm.IdGrado,
                Grupos = vm.NombreGrupo
            };

            _context.Grupos.Add(grupo);
            _context.SaveChanges();
            return RedirectToAction(nameof(ListaGrupos));
        }

        // GET: Grupos/ActualizarGrupo/5
        [HttpGet]
        public IActionResult ActualizarGrupo(int id)
        {
            var entidad = _context.Grupos.Find(id);
            if (entidad == null) return NotFound();

            var vm = new GradoGrupoViewModel
            {
                IdGrupo = entidad.IdGrupo,
                IdGrado = entidad.IdGrado,
                NombreGrupo = entidad.Grupos!
            };

            CargarGradosEnViewBag();
            return View(vm);
        }

        // POST: Grupos/ActualizarGrupo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarGrupo(GradoGrupoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                CargarGradosEnViewBag();
                return View(vm);
            }

            var entidad = _context.Grupos.Find(vm.IdGrupo);
            if (entidad == null) return NotFound();

            entidad.IdGrado = vm.IdGrado;
            entidad.Grupos = vm.NombreGrupo;

            _context.SaveChanges();
            return RedirectToAction(nameof(ListaGrupos));
        }

        public IActionResult EliminarGrupo(int id)
        {
            // 1) Cargo el grupo junto con todas sus tablas hijas
            var grupo = _context.Grupos
                .Include(g => g.GrupoMaterias)
                .Include(g => g.DocentesGrupos)
                .Include(g => g.Alumnos)
                    .ThenInclude(a => a.AlumnosTutores) // si manejas Alumno-Tutor
                .FirstOrDefault(g => g.IdGrupo == id);

            if (grupo == null)
                return NotFound();

            // 2) Borro primero las relaciones en tablas intermedias
            if (grupo.GrupoMaterias.Any())
                _context.GrupoMaterias.RemoveRange(grupo.GrupoMaterias);

            if (grupo.DocentesGrupos.Any())
                _context.DocentesGrupos.RemoveRange(grupo.DocentesGrupos);

            // 3) Si tienes Alumnos en este grupo, y quieres borrarlos:
            if (grupo.Alumnos.Any())
            {
                // Si además existe AlumnoTutor, lo quitamos antes
                foreach (var alumno in grupo.Alumnos)
                {
                    if (alumno.AlumnosTutores.Any())
                        _context.AlumnosTutores.RemoveRange(alumno.AlumnosTutores);
                }
                _context.Alumnos.RemoveRange(grupo.Alumnos);
            }

            // 4) Finalmente quito el propio Grupo
            _context.Grupos.Remove(grupo);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaGrupos));
        }
    

// Helper privado para poblar el dropdown de grados
private void CargarGradosEnViewBag()
        {
            ViewBag.Grados = _context.Grados
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.Grados
                })
                .ToList();
        }
    }
}
