using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayPal.Api;
using PizzeriaVoluptas.Models.Db;
using PizzeriaVoluptas.Models;
using PizzeriaVoluptas.Models.ViewModels;
using System.Security.Claims;

namespace PizzeriaVoluptas.Controllers
{
    public class CartController : Controller
    {

       

        private PizzaVoluptasContext _context;
        private IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;
        public CartController(PizzaVoluptasContext context, IHttpContextAccessor httpContextAccessor, IConfiguration iconfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = iconfiguration;
            _context = context;
        }


        public IActionResult Index()
        {
            var result = GetDishesinCart();

            return View(result);
        }



        public IActionResult ClearCart()
        {
            Response.Cookies.Delete("Cart");

            return Redirect("/");
        }

        [HttpPost]
        public IActionResult UpdateCart([FromBody] CartViewModel request)
        {

            var dish = _context.Dishes.FirstOrDefault(x => x.Id == request.DishId);
            if (dish == null)
            {
                return NotFound();
            }

            if(dish.Qty < request.Count)
            {
                return BadRequest();
            }

            // Retrieve the list of dishes in the cart using the dedicated function
            var cartItems = GetCartItems();

            var foundDishInCart = cartItems.FirstOrDefault(x => x.DishId == request.DishId);

            // If the dish is found, it means it is in the cart, and the user intends to change the quantity
            if (foundDishInCart == null)
            {
                var newCartItem = new CartViewModel() { };
                newCartItem.DishId = request.DishId;
                newCartItem.Count = request.Count;

                cartItems.Add(newCartItem);
            }
            else
            {
                // If greater than zero, it means the user wants to update the quantity; otherwise, it will be removed from the cart.
                if (request.Count > 0)
                {
                    foundDishInCart.Count = request.Count;
                }
                else
                {
                    cartItems.Remove(foundDishInCart);
                }
            }

            var json = JsonConvert.SerializeObject(cartItems);

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(7);
            Response.Cookies.Append("Cart", json, option);

            var result = cartItems.Sum(x => x.Count);

            return Ok(result);
        }

        public List<CartViewModel> GetCartItems()
        {
            List<CartViewModel> cartList = new List<CartViewModel>();

            var prevCartItemsString = Request.Cookies["Cart"];

            
            if (!string.IsNullOrEmpty(prevCartItemsString))
            {
                cartList = JsonConvert.DeserializeObject<List<CartViewModel>>(prevCartItemsString);
            }

            return cartList;
        }

        public List<DishCartViewModel> GetDishesinCart() 
        {
            var cartItems = GetCartItems();

            if (!cartItems.Any())
            {
                return null;
            }

            var cartItemDishIds = cartItems.Select(x => x.DishId).ToList();
            // Load dishes into memory
            var dishes = _context.Dishes
                .Where(p => cartItemDishIds.Contains(p.Id))
                .ToList();

            // Create the DishCartViewModel list

            List<DishCartViewModel> result = new List<DishCartViewModel>();
            foreach (var item in dishes)
            {
                var newItem = new DishCartViewModel
                {
                    Id = item.Id,
                    ImageName = item.ImageName,
                    Price = item.Price - (item.Discount ?? 0),
                    Title = item.Title,
                    Count = cartItems.Single(x => x.DishId == item.Id).Count,
                    RowSumPrice = (item.Price - (item.Discount ?? 0)) * cartItems.Single(x => x.DishId == item.Id).Count,
                };

                result.Add(newItem);
            }

            return result;
        }

        public IActionResult SmallCart()
        {
            var result = GetDishesinCart();
            return PartialView(result);
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var order = new Models.Db.Order();

            var shipping = _context.Settings.First().Shipping;
            if (shipping != null)
            {
                order.Shipping = shipping;
            }

            ViewData["Dishes"] = GetDishesinCart();
            return View(order);
        }


        [Authorize]
        [HttpPost]
        public IActionResult ApplyCouponCode([FromForm] string couponCode)
        {
            var order = new Models.Db.Order();

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode);

            if (coupon != null)
            {
                order.CouponCode = coupon.Code;
                order.CouponDiscount = coupon.Discount;
            }
            else
            {
                ViewData["Dishes"] = GetDishesinCart();
                TempData["message"] = "Coupon not exitst";
                return View("Checkout", order);
            }

            var shipping = _context.Settings.First().Shipping;
            if (shipping != null)
            {
                order.Shipping = shipping;
            }

            ViewData["Dishes"] = GetDishesinCart();
            return View("Checkout", order);
        }


        


        [Authorize]
        [HttpPost]
        public IActionResult Checkout(Models.Db.Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Dishes"] = GetDishesinCart();
                return View(order);
            }

            //-------------------------------------------------------
            // Check and find coupon
            if (!string.IsNullOrEmpty(order.CouponCode))
            {
                var coupon = _context.Coupons.FirstOrDefault(c => c.Code == order.CouponCode);
                if (coupon != null)
                {
                    order.CouponCode = coupon.Code;
                    order.CouponDiscount = coupon.Discount;
                }
                else
                {
                    TempData["message"] = "Coupon not exists";
                    ViewData["Dishes"] = GetDishesinCart();
                    return View(order);
                }
            }

            var dishes = GetDishesinCart();

            order.Shipping = _context.Settings.First().Shipping;
            order.CreateDate = DateTime.Now;
            order.SubTotal = dishes.Sum(x => x.RowSumPrice);
            order.Total = (order.SubTotal + order.Shipping ?? 0);
            order.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (order.CouponDiscount != null)
            {
                order.Total -= order.CouponDiscount;
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            //-------------------------------------------------------
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var item in dishes)
            {
                OrderDetail orderDetailItem = new OrderDetail()
                {
                    Count = item.Count,
                    DishTitle = item.Title,
                    DishPrice = (decimal)item.Price,
                    OrderId = order.Id,
                    DishId = item.Id
                };
                orderDetails.Add(orderDetailItem);
            }

            _context.OrderDetails.AddRange(orderDetails);

            //-------------------------------------------------------
            // Reduce QTY
            var orderDetailsFromDb = _context.OrderDetails.Where(x => x.OrderId == order.Id).ToList();
            var dishIds = orderDetailsFromDb.Select(x => x.DishId);
            var dishesToUpdate = _context.Dishes.Where(x => dishIds.Contains(x.Id)).ToList();

            foreach (var item in dishesToUpdate)
            {
                item.Qty -= orderDetailsFromDb.FirstOrDefault(x => x.DishId == item.Id).Count;
            }

            _context.Dishes.UpdateRange(dishesToUpdate);
            //-------------------------------------------------------

            _context.SaveChanges();

            // Redirect to PayPal


            return Redirect("/Cart/RedirectToPayPal?orderId=" + order.Id);
        }




        public ActionResult RedirectToPayPal(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return View("PaymentFailed");
            }

            var orderDetails = _context.OrderDetails.Where(x => x.OrderId == orderId).ToList();

            var clientId = _configuration.GetValue<string>("PayPal:Key");
            var clientSecret = _configuration.GetValue<string>("PayPal:Secret");
            var mode = _configuration.GetValue<string>("PayPal:mode");
            var apiContext = PaypalConfiguration.GetAPIContext(clientId, clientSecret, mode);

            try
            {
                string baseURI = $"{Request.Scheme}://{Request.Host}/cart/PaypalReturn?";
                var guid = Guid.NewGuid().ToString();

                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
      {
          new Transaction
          {
              description = $"Order {order.Id}",
              invoice_number = Guid.NewGuid().ToString(),
              amount = new Amount
              {
                  currency = "USD",
                  total = order.Total?.ToString("F"),
                  
              },

             
          }
      },
                    redirect_urls = new RedirectUrls
                    {
                        cancel_url = $"{baseURI}&Cancel=true",
                        return_url = $"{baseURI}orderId={order.Id}"
                    }
                };
               

                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel.ToLower() == "approval_url")?.href;

                _httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);
                return Redirect(approvalUrl);
            }
            catch (Exception e)
            {
                return View("PaymentFailed");
            }
        }

        public ActionResult PaypalReturn(int orderId, string PayerID)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return View("PaymentFailed");
            }

            var clientId = _configuration.GetValue<string>("PayPal:Key");
            var clientSecret = _configuration.GetValue<string>("PayPal:Secret");
            var mode = _configuration.GetValue<string>("PayPal:mode");
            var apiContext = PaypalConfiguration.GetAPIContext(clientId, clientSecret, mode);

            try
            {
                var paymentId = _httpContextAccessor.HttpContext.Session.GetString("payment");
                var paymentExecution = new PaymentExecution { payer_id = PayerID };
                var payment = new Payment { id = paymentId };

                var executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment.state.ToLower() != "approved")
                {
                    return View("PaymentFailed");
                }

                Response.Cookies.Delete("Cart");
                // Save the PayPal transaction ID and update order status
                order.TransId = executedPayment.transactions[0].related_resources[0].sale.id;
                order.Status = executedPayment.state.ToLower();
                
                _context.SaveChanges();

                ViewData["orderId"] = order.Id;

                return View("PaymentSuccess");
            }
            catch (Exception)
            {
                return View("PaymentFailed");
            }
        }



    }
}
