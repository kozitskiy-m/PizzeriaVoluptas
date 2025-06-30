using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzeriaVoluptas.Models.Db;
using System.Security.Claims;

namespace PizzeriaVoluptas.Areas.User.Controllers
{
    [Authorize]
    [Area("User")]
    public class HomeController : Controller
    {

        private readonly PizzaVoluptasContext _context;

        public HomeController(PizzaVoluptasContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            return View(user);
        }
    }
}
