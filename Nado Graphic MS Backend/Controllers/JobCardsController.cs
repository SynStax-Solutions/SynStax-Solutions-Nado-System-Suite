using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //Any logged-in user can use these endpoints (all roles work with job cards).
    [Authorize]
    public class JobCardsController : ControllerBase
    {
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//