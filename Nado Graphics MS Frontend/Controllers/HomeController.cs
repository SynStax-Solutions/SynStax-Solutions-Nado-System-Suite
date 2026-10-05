using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    public class HomeController : Controller
    {
        //This sends the user to the login page, or to their dashboard if they are logged in.
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            var (action, controller) = AppRoles.DashboardFor(User.FindFirst(ClaimTypes.Role)?.Value);
            return RedirectToAction(action, controller);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
// ------------------------- END OF FILE ------------------------- //