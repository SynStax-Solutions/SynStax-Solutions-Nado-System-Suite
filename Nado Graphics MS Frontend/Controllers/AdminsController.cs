using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Only the Manager and PA/Admin can open these pages. Graphic Designers can't.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class AdminsController : Controller
    {
        private readonly EnquiryService _enquiryService;

        public AdminsController(EnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }


        //This shows the Admin dashboard.
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Enquiries = await _enquiryService.GetEnquiriesAsync();
            return View();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//