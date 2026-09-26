using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Only the Manager and PA/Admin can open these pages. Graphic Designers can't.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class AdminsController : Controller
    {
        //This shows the Admin dashboard.
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//