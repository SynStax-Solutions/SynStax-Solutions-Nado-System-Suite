using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Any logged-in user can open these pages, since Graphic Designer is the lowest role.
    [Authorize]
    public class DesignersController : Controller
    {
        //This shows the Designer dashboard.
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//