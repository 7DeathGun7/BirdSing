using System.Linq;
using BirdSing.Data;
using BirdSing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")]
    public class DocentesGruposController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DocentesGruposController(ApplicationDbContext context)
            => _context = context;

        // GET: /DocentesGrupos/Lista
        public IActionResult Lista()
        {
            var lista = _context.DocentesGrupos
                .Include(x => x.Docente).ThenInclude(d => d.Usuario)
                .Include(x => x.Grado)
                .Include(x => x.Grupo)
                .ToList();
            return View(lista);
        }

        // GET: /DocentesGrupos/Registro
        public IActionResult Registro()
        {
            CargarDropdowns();
            return View(new DocenteGrupo());
        }

        // POST: /DocentesGrupos/Registro
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Registro(DocenteGrupo model)
        {
            if (ModelState.IsValid)
            {
                _context.DocentesGrupos.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Lista));
            }
            CargarDropdowns();
            return View(model);
        }

        // GET: /DocentesGrupos/Actualizar?IdDocente=1&IdGrado=2&IdGrupo=3
        public async Task<IActionResult> Actualizar(int idDocente)
        {
            var model = await _context.DocentesGrupos
                .Include(dg => dg.Docente).ThenInclude(d => d.Usuario)
                .FirstOrDefaultAsync(dg => dg.IdDocente == idDocente);

            if (model == null) return NotFound();

            // Docentes disponibles
            var docentes = await _context.Docentes
                .Include(d => d.Usuario)
                .Select(d => new
                {
                    d.IdDocente,
                    Nombre = d.Usuario.NombreUsuario + " " + d.Usuario.ApellidoPaterno
                })
                .ToListAsync();

            // Cargar todos los grados
            var grados = await _context.Grados.ToListAsync();

            // Cargar grupos vinculados al grado actual
            var gruposFiltrados = await _context.Grupos
                .Where(g => g.IdGrado == model.IdGrado)
                .ToListAsync();

            ViewBag.Docentes = new SelectList(docentes, "IdDocente", "Nombre", model.IdDocente);
            ViewBag.Grados = new SelectList(grados, "IdGrado", "Grados", model.IdGrado);
            ViewBag.Grupos = new SelectList(gruposFiltrados, "IdGrupo", "Grupos", model.IdGrupo);

            return View(model);
        }



        // POST: /DocentesGrupos/Actualizar
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Actualizar(DocenteGrupo model)
        {
            if (ModelState.IsValid)
            {
                // Para PK compuesta: eliminar + reinsertar
                var old = _context.DocentesGrupos.Find(model.IdDocente, model.IdGrado, model.IdGrupo);
                if (old != null)
                {
                    _context.DocentesGrupos.Remove(old);
                    _context.DocentesGrupos.Add(model);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Lista));
                }
            }
            CargarDropdowns();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGruposPorGrado(int gradoId)
        {
            var grupos = await _context.Grupos
                .Where(g => g.IdGrado == gradoId)
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrupo.ToString(),
                    Text = g.Grupos
                })
                .ToListAsync();

            return Json(grupos);
        }

        // GET: /DocentesGrupos/Eliminar?IdDocente=1&IdGrado=2&IdGrupo=3
        public IActionResult Eliminar(int idDocente, int idGrado, int idGrupo)
        {
            var entidad = _context.DocentesGrupos.Find(idDocente, idGrado, idGrupo);
            if (entidad != null)
            {
                _context.DocentesGrupos.Remove(entidad);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Lista));
        }

        private void CargarDropdowns()
        {
            ViewBag.Docentes = _context.Docentes
                .Include(d => d.Usuario)
                .Select(d => new SelectListItem
                {
                    Value = d.IdDocente.ToString(),
                    Text = d.Usuario!.NombreUsuario
                })
                .ToList();

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
