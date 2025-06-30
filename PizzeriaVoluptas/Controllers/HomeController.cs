using Microsoft.AspNetCore.Mvc;
using PizzeriaVoluptas.Models;
using PizzeriaVoluptas.Models.Db;
using System.Diagnostics;

namespace PizzeriaVoluptas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PizzaVoluptasContext _context;

        public HomeController(ILogger<HomeController> logger, PizzaVoluptasContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var banners = _context.Banners.ToList();
            ViewData["banners"] = banners;
            //------------new dishes-------------

            var newDishes = _context.Dishes.OrderByDescending(x => x.Id).Take(8).ToList();
            ViewData["newDishes"] = newDishes;

            //-----------best selling------------




            //-----------------------------------

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
