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

        // GET: Grupos/EliminarGrupo/5
        public IActionResult EliminarGrupo(int id)
        {
            var entidad = _context.Grupos.Find(id);
            if (entidad != null)
            {
                _context.Grupos.Remove(entidad);
                _context.SaveChanges();
            }
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
