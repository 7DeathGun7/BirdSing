using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BirdSing.Controllers
{
    [Authorize(Roles = "3")]
    public class PanelTutorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
