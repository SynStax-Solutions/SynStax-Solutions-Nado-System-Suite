using Microsoft.AspNetCore.Mvc;

namespace Nado_Graphic_MS_Frontend.Controllers
{
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
