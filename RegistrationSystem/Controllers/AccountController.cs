using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RegistrationSystem.Models;

namespace RegistrationSystem.Controllers
{
    // Handles the application's custom MVC Login page
    // while continuing to use ASP.NET Core Identity.
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser>
            _signInManager;

        private readonly IStringLocalizer<SharedResource>
            _localizer;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _signInManager = signInManager;
            _localizer = localizer;
        }

        // GET: /Account/Login
        [AllowAnonymous] // lets logged out visitors access it
        [HttpGet]
        public async Task<IActionResult> Login( // asyc Task<IActionResult> allows the action to perform asynchronous identity operations
            string? returnUrl = null) // returnUrl identifies the page user originally wanted
        {
            // Uses the application's root page when there
            // is no originally requested destination.
            returnUrl ??= Url.Content("~/"); // ?? means "assign this value only if the current value is null"

            // Clears any temporary external-login cookie
            // left by an unfinished external login.
            await HttpContext.SignOutAsync(
                IdentityConstants.ExternalScheme); // it does not log out the normal application user. This protects against an unfinished Google, Microsoft, or another external login attempt leaving temporary authentication data behind

            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        // POST: /Account/Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken] // protects against forged form submissions
        public async Task<IActionResult> Login(
            LoginViewModel model) // the submitted fields are placed into LoginViewModel model
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Identity safely checks the submitted password
            // against the hashed password in AspNetUsers.
            var result =
                await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Only redirects to an address belonging
                // to this application.
                if (!string.IsNullOrWhiteSpace(
                        model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl)) // Url.IslocalUrl is an important security check. It prevents attackers from using the Login page to redirect users to an external malicious website
                {
                    return LocalRedirect(
                        model.ReturnUrl);
                }

                // Home chooses the correct destination
                // according to the logged-in user's role. this is if there is no safe destination
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            if (result.IsLockedOut) // The account is already locked
            {
                ModelState.AddModelError(
                    string.Empty,
                    _localizer["AccountLocked"]);
            }
            else if (result.RequiresTwoFactor) // The account requires two-factor authentication
            {
                ModelState.AddModelError(
                    string.Empty,
                    _localizer["TwoFactorRequired"]);
            }
            else // The email or password is incorrect
            {
                ModelState.AddModelError(
                    string.Empty,
                    _localizer["InvalidLoginAttempt"]);
            }

            return View(model);
        }
    }
}