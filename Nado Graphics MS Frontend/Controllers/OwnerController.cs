using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Only the Manager (Owner) can open these pages.
    [Authorize(Roles = AppRoles.Manager)]
    public class OwnerController : Controller
    {
        //This shows the Owner dashboard.
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//