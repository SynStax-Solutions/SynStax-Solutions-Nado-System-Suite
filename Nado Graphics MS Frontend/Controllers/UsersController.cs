using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetUsersAsync();

            return View(users);
        }

        //----------------------------------------------------------------------------------//
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViewUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _userService.CreateUserAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the user.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------------------------------------//
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
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _userService.UpdateUserAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the user.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the user.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
//-------------------------------------------<>\\<>END OF PROGRAM<>//<>-----------------------------------------//