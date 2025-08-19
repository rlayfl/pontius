using Microsoft.AspNetCore.Mvc;
using Pontius.Models;
using System.Diagnostics;

namespace Pontius.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(ILogger<StatisticsController> logger)
        {
            _logger = logger;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LogClick()
        {
            

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
