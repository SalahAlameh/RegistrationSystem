using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using System.Diagnostics;

namespace RegistrationSystem.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            // Administrators are sent to the Dashboard
            // when they enter the application.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // Regular Users are sent to the Books page.
            return RedirectToAction(
                "Index",
                "Book");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}