using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Data.Entities;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduMVC.Controllers
{
    [Authorize()]
    public class ShoppingCartController : BaseController
    {
        private readonly UserManager<EduUser> _userManager;
        private readonly EduDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ShoppingCartController(UserManager<EduUser> userManager,
            IWebHostEnvironment environment,
            EduDbContext context)
            : base(context, environment)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid courseId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var shoppingCart = CreateCart(currentUser.Id, courseId);

            return Json(new { shoppingCart });
        }

        //public IActionResult CartDetails()
        //{

        //    var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
        //    if (cart == null)
        //    {
        //        return View();
        //    }

        //    var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
        //    ViewBag.ShoppingCart = shoppingCart;

        //    return View(shoppingCart);
        //}

        public async Task<IActionResult> CartDetails()
        {
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            if (cart == null)
            {
                return View(new List<CourseSummaryVM>());
            }

            var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
            var courseDetails = new List<CourseSummaryVM>();

            var allUsers = await _userManager.Users.ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id, u => u.UserName);

            if (shoppingCart?.CourseIds != null && shoppingCart.CourseIds.Any())
            {
                courseDetails = await _context.Courses
                    .Include(c => c.Image)
                    .Where(c => shoppingCart.CourseIds.Contains(c.Id))
                    .Select(c => new CourseSummaryVM
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Price = c.Price,
                        OwnerName = userDictionary.ContainsKey(c.OwnerId) ? userDictionary[c.OwnerId] : "Unknown Owner",
                        ImagePath = FileHelper.GetImageFilePath(c)
                    })
                    .ToListAsync();
            }

            return View(courseDetails);
        }


        [HttpPost]
        public IActionResult RemoveFromCart(Guid courseId)
        {
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            if (cart != null)
            {
                var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                if (shoppingCart?.CourseIds != null && shoppingCart.CourseIds.Contains(courseId))
                {
                    shoppingCart.CourseIds.Remove(courseId);
                    HttpContext.Session.SetString(Constants.SESSION_CART, JsonConvert.SerializeObject(shoppingCart));
                }
            }

            return Ok();
        }

        public async Task<IActionResult> ReloadShoppingCart()
        {
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            if (cart == null)
            {
                return PartialView("_ShoppingCart", new List<CourseSummaryVM>());
            }

            var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
            var courseDetails = new List<CourseSummaryVM>();

            var allUsers = await _userManager.Users.ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id, u => u.UserName);

            if (shoppingCart?.CourseIds != null && shoppingCart.CourseIds.Any())
            {
                courseDetails = await _context.Courses
                    .Include(c => c.Image)
                    .Where(c => shoppingCart.CourseIds.Contains(c.Id))
                    .Select(c => new CourseSummaryVM
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Price = c.Price,
                        OwnerName = userDictionary.ContainsKey(c.OwnerId) ? userDictionary[c.OwnerId] : "Unknown Owner",
                        ImagePath = FileHelper.GetImageFilePath(c)
                    })
                    .ToListAsync();
            }

            return PartialView("_ShoppingCart", courseDetails);
        }

        public async Task<IActionResult> MakePayment()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ShoppingCart shoppingCart = null;
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            if (cart != null)
            {
                shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                if (shoppingCart != null)
                {
                    //shoppingCart.TotalPrice = _context.Courses
                    //    .Where(c => shoppingCart.CourseIds.Contains(c.Id))
                    //    .Sum(c => c.Price);

                    var orderVM = new OrderVM
                    {
                        UserName = currentUser.UserName,
                        CourseAmount = shoppingCart.CourseIds.Count,
                        TotalPrice = _context.Courses
                                        .Where(c => shoppingCart.CourseIds.Contains(c.Id))
                                        .Sum(c => c.Price),
                        OrderDate = DateTime.Now
                    };
                    return PartialView("_MakePayment", orderVM); // Pass the orderVM to the view
                }
            }
            return PartialView("_MakePayment");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderVM orderVM)
        {
            ShoppingCart shoppingCart = null;
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            if (cart != null)
            {
                shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                if (shoppingCart != null)
                {
                    //=== Step 1: Create Order ===//
                    var newOrder = new Order
                    {
                        OrderDate = orderVM.OrderDate,
                        TotalPrice = orderVM.TotalPrice,
                        UserId = shoppingCart.UserId,
                    };
                    _context.Orders.Add(newOrder);
                    //=== Step 2: Create OrderDetail ===//
                    if (shoppingCart.CourseIds != null && shoppingCart.CourseIds.Count > 0)
                    {
                        foreach (var item in shoppingCart.CourseIds)
                        {
                            var course = await _context.Courses.FindAsync(item);
                            var newOrderDetail = new OrderDetail
                            {
                                CourseId = item,
                                Price = course.Price,
                                OrderId = newOrder.Id,
                            };
                            _context.OrderDetails.Add(newOrderDetail);
                        }
                    }
                    await _context.SaveChangesAsync();

                    // Step 3: Clear the shopping cart session
                    HttpContext.Session.Remove(Constants.SESSION_CART);

                    //Return to the Main Page
                    //return RedirectToAction("ProductList", "Course");
                    var url = "Course/ProductList";
                    return Json(new { message = "Success", url });
                }
            }
            return PartialView(nameof(MakePayment));
        }

        private ShoppingCart CreateCart(String userId, Guid courseId)
        {
            ShoppingCart shoppingCart;
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);

            if (cart == null)
            {
                shoppingCart = ShoppingCartHelper.Set(userId, courseId);
            }
            else
            {
                //=== Session Shopping Cart exist ===//
                shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                if (shoppingCart != null)
                {
                    ShoppingCartHelper.AddCourse(shoppingCart, courseId);
                }
            }
            HttpContext.Session.SetString(Constants.SESSION_CART,
                    JsonConvert.SerializeObject(shoppingCart));
            return shoppingCart;
        }

        //private async Task<CourseSummaryVM?> GetProduct(Guid courseId)
        //{
        //    var course = await _context.Courses
        //        .Include(c => c.Image)
        //        .Include(c => c.PreviewMedium)
        //        .Where(c => c.Id == courseId)
        //        .SingleOrDefaultAsync();

        //    if (course == null)
        //    {
        //        return null;
        //    }

        //    var owner = await _userManager.FindByIdAsync(course.OwnerId);

        //    return new CourseSummaryVM
        //    {
        //        Id = course.Id,
        //        ImagePath = FileHelper.GetImageFilePath(course),
        //        Title = course.Title,
        //        Price = course.Price,
        //        OwnerName = owner.UserName,
        //    };
        //}
    }
}
