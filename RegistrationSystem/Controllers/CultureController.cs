using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace RegistrationSystem.Controllers
{
    // Handles changing the application's displayed language.
    public class CultureController : Controller
    {
        [AllowAnonymous] // is necessary because visitors must be able to change the Login page’s language before signing in
        [HttpPost]
        [ValidateAntiForgeryToken] // protects the POST action against forged requests. Our language form will include the corresponding token
        public IActionResult SetLanguage(
            string culture,
            string returnUrl)
        {
            // Only languages supported by the application
            // are allowed to be stored in the cookie.
            if (culture != "en" &&
                culture != "ar")
            {
                culture = "en";
            }

            // Stores the selected language in the browser
            // so it remains selected during future visits.
            Response.Cookies.Append(
                CookieRequestCultureProvider
                    .DefaultCookieName,
                CookieRequestCultureProvider
                    .MakeCookieValue(
                        new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires =
                        DateTimeOffset.UtcNow.AddYears(1),

                    // The language cookie is required for
                    // the selected interface language.
                    IsEssential = true,

                    SameSite = SameSiteMode.Lax
                });

            // Only allow redirects to pages within this
            // application, preventing an unsafe external redirect.
            if (string.IsNullOrWhiteSpace(returnUrl) ||
                !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            return LocalRedirect(returnUrl);
        }
    }
}