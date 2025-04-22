using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class GruposController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GruposController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaGrupos()
        {
            var grupos = _context.Grupos.Include(g => g.Grado).ToList();
            return View(grupos);
        }

        public IActionResult RegistroGrupo()
        {
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(new Grupo());
        }

        [HttpPost]
        public IActionResult RegistroGrupo(Grupo model)
        {
            if (ModelState.IsValid)
            {
                _context.Grupos.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaGrupos");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(model);
        }

        public IActionResult ActualizarGrupo(int id)
        {
            var grupo = _context.Grupos.Include(g => g.Grado).FirstOrDefault(g => g.IdGrupo == id);
            if (grupo == null)
            {
                return NotFound();
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(grupo);
        }

        [HttpPost]
        public IActionResult ActualizarGrupo(Grupo model)
        {
            if (ModelState.IsValid)
            {
                _context.Grupos.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaGrupos");
            }
            var grados = _context.Grados.ToList();
            ViewBag.Grados = grados.Select(g => new SelectListItem
            {
                Value = g.IdGrado.ToString(),
                Text = g.Grados
            }).ToList();
            return View(model);
        }

        public IActionResult EliminarGrupo(int id)
        {
            var grupo = _context.Grupos.Find(id);
            if (grupo != null)
            {
                _context.Grupos.Remove(grupo);
                _context.SaveChanges();
            }
            return RedirectToAction("ListaGrupos");
        }
    }
}