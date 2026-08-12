using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RegistrationSystem.Models;
using RegistrationSystem.Services;

namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")] // every action in this controller is available only to administrators
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UsersController(
            UserService userService,
            IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        // GET: /Users/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users =
                await _userService.GetAllAsync(User);

            return View(users);
        }

        // GET: /Users/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                await _userService.GetCreateModelAsync();

            return View(model);
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await _userService.PopulateCreateOptionsAsync(model);

                return View(model);
            }

            var result =
                await _userService.CreateAsync(model);

            if (!result.Succeeded)
            {
                AddErrors(result);

                await _userService.PopulateCreateOptionsAsync(model);

                return View(model);
            }

            TempData["SuccessMessage"] =
                _localizer[
                    "UserCreatedSuccessfully",
                    model.Email].Value;

            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model =
                await _userService.GetEditModelAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await _userService.PopulateEditOptionsAsync(model);

                return View(model);
            }

            var result =
                await _userService.UpdateAsync(model, User);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                AddErrors(result);

                await _userService.PopulateEditOptionsAsync(model);

                return View(model);
            }

            TempData["SuccessMessage"] =
                _localizer[
                    "UserUpdatedSuccessfully",
                    model.Email].Value;

            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/ResetPassword/5
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var model =
                await _userService.GetResetPasswordModelAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Users/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetUserPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await _userService.ResetPasswordAsync(model, User);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                AddErrors(result);

                return View(model);
            }

            TempData["SuccessMessage"] =
                _localizer[
                    "UserPasswordResetSuccessfully",
                    model.Email].Value;

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Disable/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(string id)
        {
            var result =
                await _userService.DisableAsync(id, User);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    LocalizeErrors(result.Errors);

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                _localizer["UserDeactivatedSuccessfully"].Value;

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Enable/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(string id)
        {
            var result =
                await _userService.EnableAsync(id);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    LocalizeErrors(result.Errors);

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                _localizer["UserReactivatedSuccessfully"].Value;

            return RedirectToAction(nameof(Index));
        }

        private void AddErrors(UserOperationResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    LocalizeError(error));
            }
        }

        private string LocalizeErrors(
            IEnumerable<string> errorKeys)
        {
            return string.Join(
                ", ",
                errorKeys.Select(LocalizeError));
        }

        private string LocalizeError(string errorKey)
        {
            var localizedError = _localizer[errorKey];

            // Unknown Identity errors receive a safe localized fallback
            // instead of displaying a raw resource key to the user.
            return localizedError.ResourceNotFound
                ? _localizer["UserOperationFailed"].Value
                : localizedError.Value;
        }
    }
}
