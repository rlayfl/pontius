using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using Pontius.ExperimentObjects;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace Pontius.Controllers
{
    public class ExperimentController : Controller
    {

        private readonly string _firebaseDatabaseEndpoint = "https://pontius-b5de5-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly string _firebaseExperimentsTableEndpoint;
        private readonly string _firebaseAnswersTableEndpoint;

        private readonly ILogger<HomeController> _logger;

        public ExperimentController(ILogger<HomeController> logger)
        {
            _firebaseExperimentsTableEndpoint = $"{_firebaseDatabaseEndpoint}experiments.json";
            _firebaseAnswersTableEndpoint = $"{_firebaseDatabaseEndpoint}answers.json";
            _logger = logger;
        }

        [SkipGlobalFilters]
        [HttpPost]
        public async Task<IActionResult> Answer(int usersCorrectAnswer, int usersAnswer, int usersExperimentType)
        {

            var current = User as ClaimsPrincipal;
            if (current?.Identity is not ClaimsIdentity currentIdentity)
                return Unauthorized();

            var uidClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var payload = new
            {
                UID = uidClaim,
                correctAnswer = usersCorrectAnswer,
                answer = usersAnswer,
                experimentType = usersExperimentType,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var startExperimentResponse = await httpClient.PostAsync(_firebaseAnswersTableEndpoint, content);

            if (!startExperimentResponse.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Something went wrong DEBUG." });
            }          

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> End()
        {
            var identity = (ClaimsIdentity)User.Identity!;

            void UpdateClaim(string type, string value)
            {
                var existingClaim = identity.FindFirst(type);
                if (existingClaim != null)
                    identity.RemoveClaim(existingClaim);

                identity.AddClaim(new Claim(type, value));
            }
            UpdateClaim("HasStartedExperiment", false.ToString());
            UpdateClaim("ExperimentType", string.Empty);
            UpdateClaim("HasStartedTest", false.ToString());

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

        public ExperimentType GetRandomExperimentType()
        {
            var values = Enum.GetValues(typeof(ExperimentType));
            var random = new Random();
            var value = values.GetValue(random.Next(values.Length));
            return value is not null ? (ExperimentType)value : default;
        }

        public IActionResult InProgress()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            return View();
        }

        [RedirectIfNOTAuthenticated]
        [HttpGet]
        public IActionResult Start()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Start([FromBody] Start start)
        {
            //var experimentType = GetRandomExperimentType();

            var current = User as ClaimsPrincipal;
                if (current?.Identity is not ClaimsIdentity currentIdentity)
                    return Unauthorized();

            var experimentTypeEnum = ExperimentType.InformationOverload;

            var uidClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var payload = new
            {
                UID = uidClaim,
                experimentType = experimentTypeEnum,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var startExperimentResponse = await httpClient.PostAsync(_firebaseExperimentsTableEndpoint, content);

            if (startExperimentResponse.IsSuccessStatusCode)
            {
                var responseContent = await startExperimentResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var experimentID = doc.RootElement.GetProperty("name").GetString();                

                var claims = currentIdentity.Claims.ToList();
                claims.RemoveAll(c => c.Type == "HasStartedExperiment");
                claims.Add(new Claim("HasStartedExperiment", true.ToString()));

                claims.RemoveAll(c => c.Type == "ExperimentType");
                claims.Add(new Claim("ExperimentType", experimentTypeEnum.ToString()));

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

        [RedirectIfNOTAuthenticated]
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
                var fileName = Path.GetFileNameWithoutExtension(file);

                // Replace underscores with spaces
                var name = fileName.Replace("_", " ");

                // Remove any trailing numbers (e.g., "Cardinal Mark East 1" → "Cardinal Mark East")
                name = System.Text.RegularExpressions.Regex.Replace(name, @"\d+$", "");

                // Trim in case there are extra spaces left behind
                name = name.Trim();

                // Convert to Title Case
                name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);

                MarkerBuoyType buoyType = MarkerBuoyType.Unknown;

                if (fileName.StartsWith("cardinal_mark_north", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.CardinalMarkNorth;
                else if (fileName.StartsWith("cardinal_mark_south", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.CardinalMarkSouth;
                else if (fileName.StartsWith("cardinal_mark_east", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.CardinalMarkEast;
                else if (fileName.StartsWith("cardinal_mark_west", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.CardinalMarkWest;
                else if (fileName.StartsWith("emergency_wreck_mark", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.EmergencyWreckMark;
                else if (fileName.StartsWith("isolated_danger_mark", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.IsolatedDangerMark;
                else if (fileName.StartsWith("port_mark", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.PortMark;
                else if (fileName.StartsWith("safe_water", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.SafeWaterMark;
                else if (fileName.StartsWith("special_mark", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.SpecialMark;
                else if (fileName.StartsWith("starboard_mark", StringComparison.OrdinalIgnoreCase))
                    buoyType = MarkerBuoyType.StarboardMark;

                var relativePath = file.Replace("wwwroot", "").Replace("\\", "/");

                experimentTestViewModel.MarkerBuoys.Add(new MarkerBuoy
                {
                    Name = buoyType.GetDescription(),
                    ImageURL = relativePath,
                    MarkerBuoyImageType = MarkerBuoyImageType.Real,
                    MarkerBuoyType = buoyType
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
    }
}