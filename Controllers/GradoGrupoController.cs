using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]
    public class GradoGrupoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradoGrupoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult RegistroGradoGrupo()
        {
            ViewBag.Grados = _context.Grados.Where(g => g.Activo).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarGrado(string nombreGrado)
        {
            if (_context.Grados.Any(g => g.Grados == nombreGrado && g.Activo))
            {
                ModelState.AddModelError("", "El grado ya existe.");
            }
            else
            {
                _context.Grados.Add(new Grado { Grados = nombreGrado });
                _context.SaveChanges();
            }
            return RedirectToAction("RegistroGradoGrupo");
        }

        [HttpPost]
        public IActionResult RegistrarGrupo(int idGrado, string nombreGrupo)
        {
            if (!_context.Grados.Any(g => g.IdGrado == idGrado && g.Activo))
                return NotFound();

            if (_context.Grupos.Any(g => g.IdGrado == idGrado && g.Grupos == nombreGrupo && g.Activo))
            {
                ModelState.AddModelError("", "Ese grupo ya existe en el grado seleccionado.");
            }
            else
            {
                _context.Grupos.Add(new Grupo { IdGrado = idGrado, Grupos = nombreGrupo });
                _context.SaveChanges();
            }
            return RedirectToAction("RegistroGradoGrupo");
        }

        public IActionResult ListaGradoGrupo()
        {
            var lista = _context.Grados
                .Include(g => g.Grupos.Where(gr => gr.Activo))
                .Where(g => g.Activo)
                .ToList();

            return View(lista);
        }

        [HttpPost]
        public IActionResult EliminarGrupo(int id)
        {
            var grupo = _context.Grupos.Find(id);
            if (grupo != null)
            {
                grupo.Activo = false;
                _context.SaveChanges();
            }
            return RedirectToAction("ListaGradoGrupo");
        }

        [HttpPost]
        public IActionResult EliminarGrado(int id)
        {
            var grado = _context.Grados.Include(g => g.Grupos).FirstOrDefault(g => g.IdGrado == id);
            if (grado != null)
            {
                grado.Activo = false;
                foreach (var grupo in grado.Grupos)
                {
                    grupo.Activo = false;
                }
                _context.SaveChanges();
            }
            return RedirectToAction("ListaGradoGrupo");
        }

        [HttpGet]
        public IActionResult EditarGradoGrupo(int id)
        {
            var grado = _context.Grados
                .Include(g => g.Grupos.Where(gr => gr.Activo))
                .FirstOrDefault(g => g.IdGrado == id && g.Activo);

            if (grado == null) return NotFound();

            return View(grado);
        }

        [HttpPost]
        public IActionResult EditarGradoGrupo(int IdGrado, string NombreGrado, List<Grupo> Grupos, string NuevoGrupo)
        {
            var grado = _context.Grados
                .Include(g => g.Grupos)
                .FirstOrDefault(g => g.IdGrado == IdGrado);

            if (grado == null) return NotFound();

            if (_context.Grados.Any(g => g.IdGrado != IdGrado && g.Grados == NombreGrado && g.Activo))
            {
                ModelState.AddModelError("NombreGrado", "Ya existe un grado con ese nombre.");
                return View(grado);
            }

            grado.Grados = NombreGrado;

            foreach (var grupoEditado in Grupos)
            {
                var grupo = grado.Grupos.FirstOrDefault(g => g.IdGrupo == grupoEditado.IdGrupo);
                if (grupo != null)
                {
                    if (_context.Grupos.Any(g => g.IdGrupo != grupo.IdGrupo && g.IdGrado == IdGrado && g.Grupos == grupoEditado.Grupos && g.Activo))
                    {
                        ModelState.AddModelError("Grupos", $"Grupo duplicado: {grupoEditado.Grupos}");
                        return View(grado);
                    }
                    grupo.Grupos = grupoEditado.Grupos;
                }
            }

            if (!string.IsNullOrWhiteSpace(NuevoGrupo))
            {
                if (!_context.Grupos.Any(g => g.Grupos == NuevoGrupo && g.IdGrado == IdGrado && g.Activo))
                {
                    _context.Grupos.Add(new Grupo
                    {
                        IdGrado = IdGrado,
                        Grupos = NuevoGrupo
                    });
                }
                else
                {
                    ModelState.AddModelError("NuevoGrupo", "Ese grupo ya existe en el grado.");
                    return View(grado);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("ListaGradoGrupo");
        }
    }
}
