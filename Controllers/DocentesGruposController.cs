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
        public IActionResult Actualizar(int idDocente, int idGrado, int idGrupo)
        {
            var entidad = _context.DocentesGrupos.Find(idDocente, idGrado, idGrupo);
            if (entidad == null) return NotFound();
            CargarDropdowns();
            return View(entidad);
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
