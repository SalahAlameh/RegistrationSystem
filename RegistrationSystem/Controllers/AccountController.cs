using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using RegistrationSystem.Models;
using System.Text;
using System.Text.Encodings.Web;

namespace RegistrationSystem.Controllers
{
    // Handles the application's custom MVC account pages
    // while continuing to use ASP.NET Core Identity securely.
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser>
            _userManager;

        private readonly SignInManager<IdentityUser>
            _signInManager;

        private readonly IEmailSender
            _emailSender;

        private readonly IStringLocalizer<SharedResource>
            _localizer;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _localizer = localizer;
        }

        // GET: /Account/Login
        [AllowAnonymous] // lets logged out visitors access it
        [HttpGet]
        public async Task<IActionResult> Login( // asyc Task<IActionResult> allows the action to perform asynchronous identity operations
            string? returnUrl = null) // returnUrl identifies the page user originally wanted
        {
       
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

        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(
            string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var viewModel = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        // POST: /Account/Register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                if (!string.IsNullOrWhiteSpace(
                        model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    LocalizeIdentityError(error.Code));
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: /Account/ForgotPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.Email);

            // Always show the same confirmation page so visitors cannot
            // discover whether a particular email address has an account.
            if (user == null)
            {
                return RedirectToAction(
                    nameof(ForgotPasswordConfirmation));
            }

            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(resetToken));

            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new
                {
                    code = encodedToken,
                    email = model.Email
                },
                Request.Scheme)!;

            await _emailSender.SendEmailAsync(
                model.Email,
                _localizer["PasswordResetEmailSubject"],
                _localizer[
                    "PasswordResetEmailBody",
                    HtmlEncoder.Default.Encode(callbackUrl)]);

            return RedirectToAction(
                nameof(ForgotPasswordConfirmation));
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(
            string? code = null,
            string? email = null)
        {
            var model = new ResetPasswordViewModel
            {
                Code = code ?? string.Empty,
                Email = email ?? string.Empty
            };

            return View(model);
        }

        // POST: /Account/ResetPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.Email);

            if (user == null)
            {
                return RedirectToAction(
                    nameof(ResetPasswordConfirmation));
            }

            string decodedToken;

            try
            {
                decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(model.Code));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _localizer["InvalidToken"]);

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(
                    nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    LocalizeIdentityError(error.Code));
            }

            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ChangePassword
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: /Account/ChangePassword
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        LocalizeIdentityError(error.Code));
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction(
                nameof(ChangePasswordConfirmation));
        }

        // GET: /Account/ChangePasswordConfirmation
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/AccessDenied
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: /Account/Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Book");
        }

        private string LocalizeIdentityError(string errorCode)
        {
            var localizedError = _localizer[errorCode];

            return localizedError.ResourceNotFound
                ? _localizer["UserOperationFailed"].Value
                : localizedError.Value;
        }

    }
}
