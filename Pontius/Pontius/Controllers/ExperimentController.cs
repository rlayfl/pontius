using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using Pontius.ExperimentObjects;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Pontius.Controllers
{
    public class ExperimentController : Controller
    {

        private readonly string _firebaseDatabaseEndpoint = "https://pontius-b5de5-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly string _firebaseExperimentsTableEndpoint;

        private readonly ILogger<HomeController> _logger;

        public ExperimentController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _firebaseExperimentsTableEndpoint = $"{_firebaseDatabaseEndpoint}experiments.json";
        }

        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            return View();
        }

        // In this software we are treating an experiment as a user account
        // Starting an experiment will "log in" and start a session
        [HttpGet]
        [RedirectIfAuthenticated]
        public IActionResult Start()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Start([FromBody] Start start)
        {

            var debugUsername = "TestUsername";
            var debugPassword = "TestPassword";

            var payload = new
            {
                username = debugUsername,
                password = debugPassword,
                experimentType = GetRandomExperimentType()
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var registerResponse = await httpClient.PostAsync(_firebaseExperimentsTableEndpoint, content);

            if (registerResponse.IsSuccessStatusCode)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, debugUsername),
                    new Claim(ClaimTypes.Name, debugUsername),
                    new Claim(ClaimTypes.Role, "Legionnaire"),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            return Json(new { success = true, message = "DEBUG SUCCESS." });
        }

        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ExperimentType GetRandomExperimentType()
        {
            var values = Enum.GetValues(typeof(ExperimentType));
            var random = new Random();
            var value = values.GetValue(random.Next(values.Length));
            return value is not null ? (ExperimentType)value : default;
        }
    }   
}