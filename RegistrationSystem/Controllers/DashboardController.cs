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
        public IActionResult Index(
            int trendMonths = 12)
        {
            // Passes the administrator's selected chart period
            // to the dashboard service.
            var dashboard =
                _dashboardService.GetDashboard(trendMonths);

            return View(dashboard);
        }
    }
}