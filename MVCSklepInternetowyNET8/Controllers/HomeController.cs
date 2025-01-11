using Microsoft.AspNetCore.Mvc;
using MVCSklepInternetowyNET8.Models;
using System.Diagnostics;

namespace MVCSklepInternetowyNET8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OnlineShopContext _context;

        public HomeController(ILogger<HomeController> logger, OnlineShopContext context)
        {
            _logger = logger;
            _context = context; 
        }

        public IActionResult Index()
        {
            var visitCount = _context.Visits.Count();
            ViewBag.VisitCount = visitCount;
            return View();
        }


        public IActionResult Privacy()
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
