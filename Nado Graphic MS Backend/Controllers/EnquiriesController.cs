using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnquiriesController : ControllerBase
    {
        private readonly EnquiryService _enquiryService;

        public EnquiriesController(EnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }
        //--------------------------------------------------------------------------------------------------------------
        //Public endpoint, no [Authorize]. This is the only route the marketing
        //site is allowed to call, and only this one carries the CORS policy.
        [HttpPost]
        [EnableCors("PublicSite")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitEnquiry(EnquiryRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Unable to submit enquiry." });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var success = await _enquiryService.SubmitEnquiryAsync(request, ipAddress);

            if (!success)
            {
                //doesn't reveal whether it was validation,
                //rate limiting, or something else, so it can't be used to probe the system.
                return BadRequest(new { message = "Unable to submit enquiry." });
            }

            return Ok(new { message = "Enquiry received." });
        }
        //--------------------------------------------------------------------------------------------------------------
        //for the PA/Manager to view inside the BMS
        //same role restrictions as the rest of the system.
        [HttpGet]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
        public async Task<ActionResult<IEnumerable<Enquiry>>> GetEnquiries()
        {
            var enquiries = await _enquiryService.GetEnquiriesAsync();
            return Ok(enquiries);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpGet("{id}")]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
        public async Task<ActionResult<Enquiry>> GetEnquiry(int id)
        {
            var enquiry = await _enquiryService.GetEnquiryAsync(id);

            if (enquiry == null)
            {
                return NotFound();
            }

            return Ok(enquiry);
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------