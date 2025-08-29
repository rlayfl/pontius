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
            _firebaseExperimentsTableEndpoint = $"{_firebaseDatabaseEndpoint}experiments.json";
            _logger = logger;
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

            return RedirectToAction("ExperimentEnded", "Experiment");
        }

        public IActionResult ExperimentEnded()
        {
            return View();
        }


        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult InProgress()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Start()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Start([FromBody] Start start)
        {

            var debugUsername = "TestUsername";
            var debugPassword = "TestPassword";
            //var experimentType = GetRandomExperimentType();

            var experimentType = "InformationOverload";

            var payload = new
            {
                username = debugUsername,
                password = debugPassword,
                experimentType = experimentType
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var startExperimentResponse = await httpClient.PostAsync(_firebaseExperimentsTableEndpoint, content);

            if (startExperimentResponse.IsSuccessStatusCode)
            {
                var current = User as ClaimsPrincipal;
                if (current?.Identity is not ClaimsIdentity currentIdentity)
                    return Unauthorized();

                var claims = currentIdentity.Claims.ToList();
                claims.RemoveAll(c => c.Type == "HasStartedExperiment");
                claims.Add(new Claim("HasStartedExperiment", true.ToString()));

                claims.RemoveAll(c => c.Type == "ExperimentType");
                claims.Add(new Claim("ExperimentType", experimentType.ToString()));

                var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal);

                HttpContext.User = newPrincipal;

                return Json(new { success = true, redirectUrl = Url.Action("overview", "buoys") });
            }

            return Json(new { success = false, message = "Something went wrong DEBUG." });
        }

        [HttpGet]
        public async Task<IActionResult> SetTestHasStarted()
        {
            if (User.Identity is not ClaimsIdentity currentIdentity)
                return Unauthorized();

            var claims = currentIdentity.Claims.ToList();
            claims.RemoveAll(c => c.Type == "HasStartedTest");
            claims.Add(new Claim("HasStartedTest", bool.TrueString));

            var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal);
            HttpContext.User = newPrincipal;

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Test()
        {
            var experimentTestViewModel = new ExperimentTestViewModel();

            // Try to load from session first
            var buoyOrder = HttpContext.Session.GetString("BuoyOrder");

            List<string> files;
            if (string.IsNullOrEmpty(buoyOrder))
            {
                // No session order yet → shuffle and save
                var random = new Random();
                files = Directory.GetFiles("wwwroot/images/buoys/real/", "*.png")
                                .OrderBy(f => random.Next())
                                .ToList();

                // Save the order in session
                HttpContext.Session.SetString("BuoyOrder", string.Join("|", files));
            }
            else
            {
                // Load existing order from session
                files = buoyOrder.Split('|').ToList();
            }

            foreach (var file in files)
            {
                var relativePath = file.Replace("wwwroot", "").Replace("\\", "/");

                experimentTestViewModel.MarkerBuoys.Add(new MarkerBuoy
                {
                    Name = "Test",
                    ImageURL = relativePath,
                    MarkerBuoyImageType = MarkerBuoyImageType.Real
                });
            }

            return View(experimentTestViewModel);
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