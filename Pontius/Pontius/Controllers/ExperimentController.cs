using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using Pontius.ExperimentObjects;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

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

        [HttpGet]
        public async Task<IActionResult> End()
        {

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, User.Identity?.Name ?? string.Empty),
                new(ClaimTypes.Name, User.Identity?.Name ?? string.Empty),
                new(ClaimTypes.Role, "Legionnaire"),
                new("HasStartedExperiment", false.ToString()),
                new("ExperimentType", string.Empty),
                new("HasStartedTest", false.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.User = principal;

            return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> Start([FromBody] Start start)
        {

            var debugUsername = "TestUsername";
            var debugPassword = "TestPassword";
            var experimentType = GetRandomExperimentType();

            var payload = new
            {
                username = debugUsername,
                password = debugPassword,
                experimentType = experimentType
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var registerResponse = await httpClient.PostAsync(_firebaseExperimentsTableEndpoint, content);

            if (registerResponse.IsSuccessStatusCode)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, debugUsername),
                    new(ClaimTypes.Name, debugUsername),
                    new(ClaimTypes.Role, "Legionnaire"),
                    new("HasStartedExperiment", true.ToString()),
                    new("ExperimentType", experimentType.ToString()),
                    new("HasStartedTest", false.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Json(new { success = true, redirectUrl = Url.Action("overview", "buoys") });
            }

            return Json(new { success = false, message = "Something went wrong DEBUG." });
        }

        [HttpGet]
        public IActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Test([FromBody] Test test)
        {
            var current = User as ClaimsPrincipal;
            if (current?.Identity is not ClaimsIdentity currentIdentity)
            return Unauthorized();

            var claims = currentIdentity.Claims.ToList();
            claims.RemoveAll(c => c.Type == "HasStartedTest");
            claims.Add(new Claim("HasStartedTest", bool.TrueString));

            var newIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
            );

            var newPrincipal = new ClaimsPrincipal(newIdentity);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            newPrincipal
            );

            HttpContext.User = newPrincipal;

            return Json(new { success = true, redirectUrl = Url.Action("test", "experiment") });
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