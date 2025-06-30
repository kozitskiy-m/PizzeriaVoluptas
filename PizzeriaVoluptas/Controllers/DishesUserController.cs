using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzeriaVoluptas.Models.Db;
using System.Text.RegularExpressions;

namespace PizzeriaVoluptas.Controllers
{
    public class DishesUserController : Controller
    {
        private readonly PizzaVoluptasContext _context;

        public DishesUserController(PizzaVoluptasContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Dish> dishes = _context.Dishes.OrderByDescending(x  => x.Id).ToList();  
            return View(dishes);
        }


        public IActionResult SearchDishes(string SearchText)
        {
            var dishes = _context.Dishes.Where(x=> EF.Functions.Like(x.Title, "%" +  SearchText + "%") ||
            EF.Functions.Like(x.Ingredients, "%" + SearchText + "%")).OrderBy(x => x.Title).ToList();


            return View("Index",   dishes);
        }



        public IActionResult DishDetails(int id)
        {
            Dish? dish = _context.Dishes.FirstOrDefault(x=>x.Id == id);
            //-------------------
            if (dish == null)
            {
                return NotFound();
            }
            //-------------------
            ViewData["gallery"] = _context.DishGaleries.Where(x=>x.DishId == id).ToList();
            //-------------------
            ViewData["NewDishes"] = _context.Dishes.Where(x=>x.Id!=id).Take(6).OrderByDescending(x => x.Id).ToList();
            //-------------------

            ViewData["comments"] = _context.Comments.Where(x => x.DishId == id).OrderByDescending(x => x.CreateDate).ToList();



            return View(dish);
        }

        [HttpPost] //використав щоб дані надсилалися більш безпечним шляхом
        public IActionResult SubmitComment(string name, string email, string comment, int dishId)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(comment) && dishId != 0)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"); //резулярний вираз для перевірки email
                Match match = regex.Match(email);
                if (!match.Success)
                {
                    TempData["ErrorMessage"] = "Email is not valid";
                    return Redirect("/dishesuser/DishDetails/" + dishId);
                }

                Comment newComment = new Comment();
                newComment.Name = name;
                newComment.Email = email;
                newComment.CommentText = comment;
                newComment.DishId = dishId;
                newComment.CreateDate = DateTime.Now;

                _context.Comments.Add(newComment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Youre comment submited success fully";
                return Redirect("/dishesuser/DishDetails/" + dishId);
            }
            else
            {
                TempData["ErrorMessage"] = "Please complete youre information";
                return Redirect("/dishesuser/DishDetails/" + dishId);
            }

        }





    }
}
