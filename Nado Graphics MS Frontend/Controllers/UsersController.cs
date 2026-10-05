using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Only the Manager (Owner) and PA/Admin can manage users.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class UsersController : Controller
    {
        //The below line stores the UserService.
        private readonly UserService _userService;

        //The constructor receives the UserService using dependency injection.
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        //These get the logged-in user's ID and role from the login cookie.
        private int CurrentUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
        private string? CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value;

        //----------------------------------------------------------------------------------//
        //This shows the list of all users.
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetUsersAsync();

            return View(users);
        }

        //----------------------------------------------------------------------------------//
        //This shows the create user page.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //----------------------------------------------------------------------------------//
        //This runs when the create user form is submitted.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViewUserModel model)
        {
            //This checks the form was filled in properly.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //This sends the new user to the API.
            var (success, message) = await _userService.CreateUserAsync(model);

            //If it failed, the error from the API is shown on the page.
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            //If it worked, a success message is shown on the user list.
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------------------------------------//
        //This shows one user's details.
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        //----------------------------------------------------------------------------------//
        //This shows the edit page for a user.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            //This stops a PA/Admin from editing a Manager or another PA/Admin.
            //Everyone can still edit their own account.
            if (user.UserID != CurrentUserId && !AppRoles.CanManage(CurrentUserRole, user.UserRole))
            {
                return Forbid();
            }

            //This fills the edit form with the user's current details.
            var model = new EditUserModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserRole = user.UserRole,
                IsActive = user.IsActive
            };

            return View(model);
        }

        //----------------------------------------------------------------------------------//
        //This runs when the edit form is submitted.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //This sends the changes to the API.
            var (success, message) = await _userService.UpdateUserAsync(model);

            //If it failed, the error is shown on the page.
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------------------------------------//
        //This deletes a user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _userService.DeleteUserAsync(id);

            //This shows a green or red message on the user list.
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------------------------------------//
        //This sends a new verification email to a user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(int id)
        {
            var (success, message) = await _userService.ResendVerificationAsync(id);

            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
//-------------------------------------------<>\\<>END OF PROGRAM<>//<>-----------------------------------------//