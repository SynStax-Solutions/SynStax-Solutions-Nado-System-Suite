using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Account.
    [Route("api/[controller]")]

    //Any logged-in user can use these endpoints (all roles can manage their own account).
    [Authorize]
    public class AccountController : ControllerBase
    {
        //The below line stores the UserService.
        private readonly UserService _userService;

        //The constructor receives the UserService using dependency injection.
        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        //This gets the logged-in user's ID from their JWT.
        private int CurrentUserId => int.TryParse(User.FindFirst(AppClaims.UserId)?.Value, out var id) ? id : 0;
        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Account/change-password.
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            //Sending the logged-in user's ID and the passwords to the UserService.
            var result = await _userService.ChangePasswordAsync(CurrentUserId, request);

            //Checking if the change failed.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response with the reason.
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response.
            return Ok(new { message = result.Message });
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//