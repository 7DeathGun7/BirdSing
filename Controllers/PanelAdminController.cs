using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "1")] // Solo permite el acceso a administradores
    public class PanelAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Tutores()
        {
            return RedirectToAction("ListaTutores", "Tutores");
        }

        public IActionResult Docentes()
        {
            return RedirectToAction("ListaDocentes", "Docentes");
        }

        public IActionResult Grados()
        {
            return RedirectToAction("ListaGrados", "Grados");
        }

        public IActionResult Grupos()
        {
            return RedirectToAction("ListaGrupos", "Grupos");
        }

        public IActionResult Materias()
        {
            return RedirectToAction("ListaMaterias", "Materias");
        }

        public IActionResult MateriasDocentes()
        {
            return RedirectToAction("ListaMateriasDocentes", "MateriasDocentes");
        }

        public IActionResult Alumnos()
        {
            return RedirectToAction("ListaAlumnos", "Alumnos");
        }

        public IActionResult AlumnosTutores()
        {
            return RedirectToAction("ListaAlumnosTutores", "AlumnosTutores");
        }
    }
}