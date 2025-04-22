using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "2")]
    public class PanelDocenteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
