using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //This controller handles login, logout, access denied and email verification.
    //There is no sign-up page. Only the Manager or PA/Admin can create accounts.
    public class AccountController : Controller
    {
        //The below line stores the AuthService.
        private readonly AuthService _authService;

        //The constructor receives the AuthService using dependency injection.
        public AccountController(AuthService authService)
        {
            _authService = authService;
        }
        //----------------------------------------------------------------------------------//
        //This shows the login page.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            //If the user is already logged in, they are sent to their dashboard.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard(User.FindFirst(ClaimTypes.Role)?.Value);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        //----------------------------------------------------------------------------------//
        //This runs when the login form is submitted.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //This checks the form was filled in properly.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //This sends the login details to the API.
            var (user, error) = await _authService.LoginAsync(model.Email, model.Password);

            //If the login failed, the error is shown on the page.
            if (user == null)
            {
                ModelState.AddModelError("", error ?? "Invalid email or password.");
                model.Password = string.Empty;
                return View(model);
            }

            //This stores the user's details and token in the login cookie.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.UserRole),
                new(ApiTokenHandler.TokenClaim, user.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //This logs the user in by creating the cookie.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    //The cookie expires at the same time as the token.
                    ExpiresUtc = DateTime.SpecifyKind(user.ExpiresAt, DateTimeKind.Utc)
                });

            //If the user was trying to open a page before logging in, they are sent back there.
            //It must be a page on our site so the link can't send them to another website.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            //Otherwise they go to their dashboard.
            return RedirectToDashboard(user.UserRole);
        }
        //----------------------------------------------------------------------------------//
        //This logs the user out by removing the cookie.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
        //----------------------------------------------------------------------------------//
        //This page shows when a user opens a page their role can't access.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        //This opens when the user clicks the link in their email.
        //It shows a button first, because some email apps open links automatically
        //to scan them, which would use up the one-time link.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail(string? token)
        {
            return View(new VerifyEmailViewModel { Token = token ?? string.Empty });
        }
        //----------------------------------------------------------------------------------//
        //This runs when the user clicks the verify button.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            //This sends the token to the API to verify the email.
            var (success, message) = await _authService.VerifyEmailAsync(model.Token);

            //This shows the result on the page.
            model.Success = success;
            model.Message = message;

            return View(model);
        }
        //This shows the change password page. Any logged-in user can open it.
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        //This runs when the change password form is submitted.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            //This checks the form was filled in properly (e.g. both new passwords match).
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //This sends the passwords to the API.
            var (success, message) = await _authService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);

            //If it failed (e.g. wrong current password), the error is shown on the page.
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(new ChangePasswordViewModel());
            }

            //If it worked, the user is logged out so they log in again with the new password.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //This message is shown on the login page.
            TempData["Success"] = message;

            return RedirectToAction(nameof(Login));
        }
        //----------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------//
        //This sends the user to the right dashboard for their role.
        private IActionResult RedirectToDashboard(string? role)
        {
            var (action, controller) = AppRoles.DashboardFor(role);
            return RedirectToAction(action, controller);
        }
    }
}
/*
 * References:

Microsoft (n.d.) Use cookie authentication without ASP.NET Core Identity. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie

Microsoft (n.d.) Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery

Microsoft (n.d.) Prevent open redirect attacks in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/preventing-open-redirects
 */
// ------------------------- END OF FILE ------------------------- //