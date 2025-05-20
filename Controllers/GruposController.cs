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

        [HttpPost]
        public async Task<IActionResult> EliminarGrupo(int id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.GrupoMaterias)
                .Include(g => g.DocentesGrupos)
                .Include(g => g.Alumnos)
                    .ThenInclude(a => a.AlumnosTutores)
                .FirstOrDefaultAsync(g => g.IdGrupo == id);

            if (grupo == null)
                return NotFound();

            // 1) Inactivar GrupoMaterias
            foreach (var gm in grupo.GrupoMaterias)
                gm.Activo = false;

            // 2) Inactivar DocentesGrupos
            foreach (var dg in grupo.DocentesGrupos)
                dg.Activo = false;

            // 3) Inactivar Alumnos y sus relaciones
            foreach (var alumno in grupo.Alumnos)
            {
                alumno.Activo = false;
                if (alumno.Usuario != null)
                    alumno.Usuario.Activo = false;

                foreach (var at in alumno.AlumnosTutores)
                    at.Activo = false;

                // También puedes inactivar avisos del alumno si lo deseas:
                var avisos = _context.Avisos
                    .Where(a => a.MatriculaAlumno == alumno.MatriculaAlumno);
                foreach (var aviso in avisos)
                    aviso.Activo = false;
            }

            // 4) Inactivar el grupo
            grupo.Activo = false;

            await _context.SaveChangesAsync();
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
