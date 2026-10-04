using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Auth.
    [Route("api/[controller]")]

    //Anyone can use these endpoints, because you aren't logged in yet when you log in or verify.
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        //The below lines store the services used for login and verification.
        private readonly AuthService _authService;
        private readonly UserService _userService;

        //The constructor receives the services using dependency injection.
        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Auth/login.
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            //Sending the login details to the AuthService to be checked.
            var (response, statusCode, error) = await _authService.LoginAsync(request);

            //Checking if the login failed.
            if (response == null)
            {
                //Returning the error code and message.
                return StatusCode(statusCode, new { message = error });
            }

            //Returning the token and user details with an HTTP 200 OK response.
            return Ok(response);
        }

        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Auth/verify-email.
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            //Sending the token to the UserService to verify the email.
            var result = await _userService.VerifyEmailAsync(request.Token);

            //Checking if the verification failed.
            if (!result.Success)
            {
                //Returning an HTTP 400 Bad Request response with the reason.
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response.
            return Ok(new { message = result.Message });
        }

        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Auth/forgot-password.
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            //This always returns the same message, whether the email has an account or not.
            var result = await _userService.ForgotPasswordAsync(request.Email);
            return Ok(new { message = result.Message });
        }

        //----------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Auth/reset-password.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            //Sending the token and new password to the UserService.
            var result = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);

            //Checking if the reset failed (wrong, used or expired link).
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            //Returning an HTTP 200 OK response.
            return Ok(new { message = result.Message });
        }
    }
}

/* References
Microsoft (n.d.) Create web APIs with ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/
Microsoft (n.d.) AllowAnonymousAttribute Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.allowanonymousattribute
Fielding, R., Nottingham, M. and Reschke, J. (2022) RFC 9110: HTTP Semantics. Internet Engineering Task Force. Available at: https://www.rfc-editor.org/rfc/rfc9110
 */
//--------------------------------------------------<>\\<>End of Program<>//<>--------------------------------------------------//