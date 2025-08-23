using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using System.Diagnostics;

namespace Pontius.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly string _firebaseDatabaseEndpoint = "https://pontius-b5de5-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly string _firebaseClicksTableEndpoint;

        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(ILogger<StatisticsController> logger)
        {
            _firebaseClicksTableEndpoint = $"{_firebaseDatabaseEndpoint}clicks.json";
            _logger = logger;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        
        [SkipGlobalFilters]
        [HttpPost]
        public async Task<IActionResult> LogClick()
        {

            var debugUsername = "TestUsername";
            var debugPassword = "TestPassword";

            var payload = new
            {
                username = debugUsername,
                password = debugPassword,
                clickTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var startExperimentResponse = await httpClient.PostAsync(_firebaseClicksTableEndpoint, content);
            

            return Json(new { success = false, message = "Something went wrong DEBUG." });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
