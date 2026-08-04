using DashboardApp.Models;
using DashboardApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            try
            {
                var records = await _apiService.GetRecordsAsync();
                return View(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard data");
                ViewBag.ApiError = ex.Message;
                return View(new List<DashboardRecord>());
            }
        }
    }
}