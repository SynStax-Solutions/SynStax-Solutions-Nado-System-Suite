using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Users.
    [Route("api/[controller]")]

    //Only a logged-in Manager (Owner) or PA/Admin can use these endpoints.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class UsersController : ControllerBase
    {
        //The below line stores the UserService used to handle user operations.
        private readonly UserService _userService;

        //The constructor receives the UserService using dependency injection.
        public UsersController(UserService userService)
        {
            //Storing the injected UserService.
            _userService = userService;
        }

        //The logged-in user's ID and role, read from their JWT.
        private int CurrentUserId => int.TryParse(User.FindFirst(AppClaims.UserId)?.Value, out var id) ? id : 0;
        private string? CurrentUserRole => User.FindFirst(AppClaims.Role)?.Value;
        //----------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Users.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            //Getting all users from the UserService.
            var users = await _userService.GetUsersAsync();

            //Returning the users with an HTTP 200 OK response.
            return Ok(users);
        }
        //----------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Users/{id}.
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            //Getting the selected user from the UserService.
            var user = await _userService.GetUserAsync(id);

            //Checking if the user does not exist.
            if (user == null)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning the user with an HTTP 200 OK response.
            return Ok(user);
        }
        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Users.
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRequest request)
        {
            //Sending the user request to the UserService for creation.
            //The logged-in user's role is passed so the service can check they may create this role.
            var result = await _userService.CreateUserAsync(request, CurrentUserRole);

            //Checking if the user creation fails.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response (with the reason).
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response with a message for the frontend to show.
            return Ok(new { message = result.Message });
        }
        //----------------------------------------------------------------------------------//
        //Handles PUT requests sent to /api/Users/{id}.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUser request)
        {
            //Sending the user ID and updated information to the UserService.
            var result = await _userService.UpdateUserAsync(id, request, CurrentUserId, CurrentUserRole);

            //Checking if the user update fails.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response (with the reason).
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response with a message for the frontend to show.
            return Ok(new { message = result.Message });
        }
        //----------------------------------------------------------------------------------//
        //Handles DELETE requests sent to /api/Users/{id}.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            //Sending the user ID to the UserService for deletion.
            var result =
                await _userService.DeleteUserAsync(id, CurrentUserId, CurrentUserRole);

            //Checking if the user deletion fails.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response (with the reason).
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response with a message for the frontend to show.
            return Ok(new { message = result.Message });
        }
        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Users/{id}/resend-verification.
        [HttpPost("{id}/resend-verification")]
        public async Task<IActionResult> ResendVerification(int id)
        {
            //Asking the UserService to send a new verification email.
            var result = await _userService.ResendVerificationAsync(id, CurrentUserId, CurrentUserRole);

            //Checking if sending fails.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response (with the reason).
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response with a message for the frontend to show.
            return Ok(new { message = result.Message });
        }
    }
}
/* References
Microsoft (n.d.) Role-based authorization in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles

Microsoft (n.d.) Create web APIs with ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/

OWASP Foundation (n.d.) Authorization Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html
 */
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//