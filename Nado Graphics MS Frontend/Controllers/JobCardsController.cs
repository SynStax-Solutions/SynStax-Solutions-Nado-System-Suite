using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Any logged-in user can open these pages (all roles work with job cards).
    [Authorize]
    public class JobCardsController : Controller
    {
        //Routes to the Job Cards index view.
        public IActionResult Index()
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        //Routes to the Create Job Card view.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        //Routes to the Edit Job Card view.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        //Routes to the Job Card details view.
        [HttpGet]
        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
// ------------------------- END OF FILE ------------------------- //
