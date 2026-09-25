using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Users.
    [Route("api/[controller]")]
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
        public async Task<ActionResult<User>> CreateUser(UserRequest request)
        {
            //Sending the user request to the UserService for creation.
            var success = await _userService.CreateUserAsync(request);

            //Checking if the user creation fails.
            if (!success)
            {
                //Returning an HTTP 400 Bad Request response.
                return BadRequest();
            }

            //Getting the newly created user from the UserService.
            var users = await _userService.GetUsersAsync();

            //Getting the most recently created user.
            var user = users.OrderByDescending(u => u.UserID).First();

            //Returning the newly created user with an HTTP 201 Created response.
            return Ok(user);
        }
        //----------------------------------------------------------------------------------//
        //Handles PUT requests sent to /api/Users/{id}.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUser request)
        {
            //Sending the user ID and updated information to the UserService.
            var success = await _userService.UpdateUserAsync(id, request);

            //Checking if the user update fails.
            if (!success)
            {
                //Returning an HTTP 400 Bad Request response.
                return BadRequest();
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
        //----------------------------------------------------------------------------------//
        //Handles DELETE requests sent to /api/Users/{id}.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            //Sending the user ID to the UserService for deletion.
            var success =
                await _userService.DeleteUserAsync(id);

            //Checking if the user deletion fails.
            if (!success)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//