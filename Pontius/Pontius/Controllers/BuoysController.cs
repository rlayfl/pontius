using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using System.Diagnostics;

namespace Pontius.Controllers
{
    public class BuoysController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public BuoysController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Overview()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
