using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Services;

namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(
            DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: /Dashboard/Index
        [HttpGet]
        public IActionResult Index()
        {
            var dashboard =
                _dashboardService.GetDashboard();

            return View(dashboard);
        }
    }
}