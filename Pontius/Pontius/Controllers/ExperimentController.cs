using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using System.Diagnostics;

namespace Pontius.Controllers
{
    public class ExperimentController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ExperimentController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            return View();
        }

        public IActionResult Start()
        {
            return View();
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
    }
}
