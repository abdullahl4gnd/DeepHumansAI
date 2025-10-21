using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DeepHumans.Models;

namespace DeepHumans.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Main page
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ===============================
        // Character Pages
        // ===============================

        public IActionResult Albert()
        {
            // Corresponds to Views/Home/albert.cshtml
            return View("albert");
        }

        public IActionResult Tupac()
        {
            return View("tupac");
        }

        public IActionResult Shakespeare()
        {
            return View("williamShakespeare");
        }

        public IActionResult Saladin()
        {
            return View("salahAlDin");
        }

        public IActionResult Churchill()
        {
            return View("winstonChurchill");
        }

        public IActionResult Hatshepsut()
        {
            return View("queenHatshepsut");
        }

        public IActionResult Saddam()
        {
            return View("sadamHussain");
        }

        public IActionResult Kanye()
        {
            return View("kanyeWest");
        }

        // ===============================
        // Error
        // ===============================

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
