using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Pontius.ExperimentObjects;
using Pontius.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Pontius.Controllers
{
    public class AccountController : Controller
    {

        private readonly string _firebaseApiKey;
        private readonly string _loginEndpoint;
        private readonly string _registerEndpoint;
        private readonly string _firebaseDatabaseEndpoint = "https://pontius-b5de5-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly string _firebaseUserAccountsTableEndpoint;

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public AccountController(ILogger<HomeController> logger, IConfiguration config)
        {
            _config = config;
            _firebaseApiKey   = _config["Firebase:ApiKey"] 
                            ?? throw new InvalidOperationException("Firebase:ApiKey missing");
            _loginEndpoint    = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}";
            _registerEndpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_firebaseApiKey}";
            _firebaseUserAccountsTableEndpoint = $"{_firebaseDatabaseEndpoint}userAccounts.json";
            _logger = logger;
        }

        [RedirectIfAuthenticated]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccount createAccount)
        {

            var payload = new
            {
                email = createAccount.EmailAddress,
                password = createAccount.Password,
                returnSecureToken = true
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var createAccountResponse = await httpClient.PostAsync(_registerEndpoint, content);

            if (createAccountResponse.IsSuccessStatusCode)
            {
                var responseContent = await createAccountResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var localId = doc.RootElement.GetProperty("localId").GetString();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, localId),
                    new("HasStartedExperiment", false.ToString()),
                    new("ExperimentType", string.Empty),
                    new("HasStartedTest", false.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);                

                var userAccountPayload = new
                {
                    localID = localId,
                    createdTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var userAccountContent = new StringContent(JsonSerializer.Serialize(userAccountPayload), System.Text.Encoding.UTF8, "application/json");

                await httpClient.PostAsync(_firebaseUserAccountsTableEndpoint, userAccountContent);


                return Json(new { success = true, redirectUrl = Url.Action("index", "home") });
            }

            return Json(new { success = false, message = "Something went wrong DEBUG." });
        }

        [RedirectIfAuthenticated]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var payload = new
            {
                email = login.EmailAddress,
                password = login.Password,
                returnSecureToken = true
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var loginResponse = await httpClient.PostAsync(_loginEndpoint, content);

            if (loginResponse.IsSuccessStatusCode)
            {
                var responseContent = await loginResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var localId = doc.RootElement.GetProperty("localId").GetString();

                var userAccountsResponse = await httpClient.GetAsync(_firebaseUserAccountsTableEndpoint);
                if (userAccountsResponse.IsSuccessStatusCode)
                {
                    var userAccountsJson = await userAccountsResponse.Content.ReadAsStringAsync();
                    using var userAccountsDoc = JsonDocument.Parse(userAccountsJson);

                    JsonElement userAccountsRoot = userAccountsDoc.RootElement;
                    string userAccountKey = null;

                    foreach (var account in userAccountsRoot.EnumerateObject())
                    {
                        if (account.Value.TryGetProperty("localID", out var idElement) && idElement.GetString() == localId)
                        {
                            userAccountKey = account.Name;
                            break;
                        }
                    }
                }



                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, localId),
                    new("HasStartedExperiment", false.ToString()),
                    new("ExperimentType", string.Empty),
                    new("HasStartedTest", false.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);



                return Json(new { success = true, redirectUrl = Url.Action("index", "home") });
            }

            return Json(new { success = false, message = "Something went wrong DEBUG." });
        }

        public IActionResult LoggedOut()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";

            return RedirectToAction("loggedOut", "account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
