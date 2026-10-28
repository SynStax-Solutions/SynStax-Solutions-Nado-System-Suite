using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Exposes billing maintenance endpoints. Only users with the Manager role can call them.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Manager)]
    public class BillingController : ControllerBase
    {
        private readonly BillingStatusService _billing;

        public BillingController(BillingStatusService billing) => _billing = billing;

        //Runs the overdue/expiry checks now and reports how many records changed.
        [HttpPost("run-checks")]
        public async Task<IActionResult> RunChecks()
        {
            var overdue = await _billing.MarkOverdueInvoicesAsync();
            var expired = await _billing.ExpireOutdatedQuotesAsync();

            return Ok(new { overdueInvoices = overdue, expiredQuotes = expired });
        }
    }
}
//-------------------------------------------END OF FILE------------------------------------------------