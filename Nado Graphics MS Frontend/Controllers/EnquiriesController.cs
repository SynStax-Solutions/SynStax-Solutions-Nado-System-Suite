using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class EnquiriesController : Controller
    {
        private readonly EnquiryService _enquiryService;

        public EnquiriesController(EnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Index()
        {
            return View(await _enquiryService.GetEnquiriesAsync());
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Details(int id)
        {
            var enquiry = await _enquiryService.GetEnquiryAsync(id);

            if (enquiry == null)
            {
                return NotFound();
            }

            if (!enquiry.IsRead)
            {
                await _enquiryService.MarkAsReadAsync(id);
            }

            return View(enquiry);
        }
    }
}