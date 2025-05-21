using BirdSing.Models;
using BirdSing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class GradosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListaGrados()
        {
            var grados = _context.Grados.ToList();
            return View(grados);
        }

        public IActionResult RegistroGrado()
        {
            return View(new Grado());
        }

        [HttpPost]
        public IActionResult RegistroGrado(Grado model)
        {
            if (ModelState.IsValid)
            {
                _context.Grados.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ListaGrados");
            }
            return View(model);
        }

        public IActionResult ActualizarGrado(int id)
        {
            var grado = _context.Grados.Find(id);
            if (grado == null)
            {
                return NotFound();
            }
            return View(grado);
        }

        [HttpPost]
        public IActionResult ActualizarGrado(Grado model)
        {
            if (ModelState.IsValid)
            {
                _context.Grados.Update(model);
                _context.SaveChanges();
                return RedirectToAction("ListaGrados");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarGrado(int id)
        {
            var grado = await _context.Grados.FindAsync(id);
            if (grado != null)
            {
                grado.Activo = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ListaGrados));
        }
    }
}